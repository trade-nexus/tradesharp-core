using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.MarketDataProvider;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.Tradier.Utility;
using TradeHub.MarketDataProvider.Tradier.ValueObject;
using Constants = TradeHub.Common.Core.Constants;

namespace TradeHub.MarketDataProvider.Tradier.Provider
{
    public class TradierMarketDataProvider : ILiveTickDataProvider, IHistoricBarDataProvider
    {
        private readonly Type _type = typeof(TradierMarketDataProvider);

        private AsyncClassLogger _logger;

        private readonly string _marketDataProviderName;

        /// <summary>
        /// Indicates if the Provider session is connected or not
        /// </summary>
        private bool _isConnected;

        /// <summary>
        /// Indicates if quotes from stream session are to be consumed or not
        /// </summary>
        private bool _consumeQuoteStream;

        /// <summary>
        /// Holds parameters required for connectivity
        /// </summary>
        private Credentials _credentials;

        /// <summary>
        /// Contains session details for Quote Stream
        /// </summary>
        private TradierStreamSession _tradierStreamSession;

        /// <summary>
        /// Contains list of symbols which are subscribe for Quote Stream
        /// </summary>
        private List<string> _subscribedSymbols;

        /// <summary>
        /// Used for holding symbols for tick subscriptions untill the request is processed
        /// </summary>
        private ConcurrentQueue<Subscribe> _symbolSubscriptionQueue;

        /// <summary>
        /// Wraps the Order symbol concurrent queue
        /// </summary>
        private BlockingCollection<Subscribe> _symbolSubscriptionCollection; 

        /// <summary>
        /// Dedicated task to consume quote messages
        /// </summary>
        private Task _quotesConsumerTask;

        /// <summary>
        /// Token source used with Quotes Consumer Task
        /// </summary>
        private CancellationTokenSource _quotesConsumerCancellationToken;

        /// <summary>
        /// Is Market Data client connected
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            return _isConnected;
        }

        #region Events

        /// <summary>
        /// Fired each time a Logon is arrived
        /// </summary>
        public event Action<string> LogonArrived;

        /// <summary>
        /// Fired each time a Logout is arrived
        /// </summary>
        public event Action<string> LogoutArrived;

        /// <summary>
        /// Fired each time a new tick arrives.
        /// </summary>
        public event Action<Tick> TickArrived;

        /// <summary>
        /// Fired when requested Historic Bar Data arrives
        /// </summary>
        public event Action<HistoricBarData> HistoricBarDataArrived;

        /// <summary>
        /// Fired each time when market data rejection arrives.
        /// </summary>
        public event Action<MarketDataEvent> MarketDataRejectionArrived;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public TradierMarketDataProvider()
        {
            // Set provider name to be used in all calls
            _marketDataProviderName = Constants.MarketDataProvider.Tradier;

            // Create object for logging details
            _logger= new AsyncClassLogger("TradierDataProvider");
            _logger.SetLoggingLevel();
            _logger.LogDirectory(Constants.DirectoryStructure.MDE_LOGS_LOCATION);

            // Initialize local list/queue for subscriptions
            _subscribedSymbols = new List<string>();
        }

        #region Connect/Disconnect

        /// <summary>
        /// Connects/Starts a client
        /// </summary>
        public bool Start()
        {
            try
            {
                // Read account credentials
                _credentials = CredentialReader.ReadCredentials("TradierParams.xml");

                if (String.IsNullOrEmpty(_credentials.ApiUrl) || String.IsNullOrWhiteSpace(_credentials.ApiUrl))
                {
                    return false;
                }

                // Create HTTP Request
                var client = new RestClient(_credentials.ApiUrl + "/user/profile");
                var request = new RestRequest(Method.GET);
                request.AddHeader("authorization", "Bearer " + _credentials.AccessToken);
                request.AddHeader("accept", "application/json");

                // Send Request
                IRestResponse response = client.Execute(request);

                var requestResult = JsonConvert.DeserializeObject<dynamic>(response.Content);

                string profileId = requestResult.profile.id.ToString();

                if (!String.IsNullOrEmpty(profileId))
                {
                    _isConnected = true;

                    // Start process to consume quotes
                    StartDataConsumer();

                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Session is available.", _type.FullName, "Start");
                    }

                    if (LogonArrived != null)
                    {
                        LogonArrived(_marketDataProviderName);
                    }

                    return _isConnected;
                }

                // Get error message
                string faultstring = requestResult.fault.faultstring.ToString();

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Session not available " + faultstring, _type.FullName, "Start");
                }

                return _isConnected;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "Start");
            }
            return false;
        }

        /// <summary>
        /// Disconnects/Stops a client
        /// </summary>
        public bool Stop()
        {
            _isConnected = false;
            _consumeQuoteStream = false;

            // Stop consuming data
            StopComsumer();

            // Clear subscription symbols
            _subscribedSymbols.Clear();

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Session closed.", _type.FullName, "Stop");
            }

            if (LogoutArrived != null)
            {
                LogoutArrived(_marketDataProviderName);
            }

            return _isConnected;
        }

        #endregion

        #region Subscribe Data

        /// <summary>
        /// Market data request message
        /// </summary>
        public bool SubscribeTickData(Subscribe subscribe)
        {
            try
            {
                // Saftey check to avoid duplication
                if (_subscribedSymbols.Contains(subscribe.Security.Symbol))
                {
                    if (_logger.IsInfoEnabled)
                    {
                        _logger.Info("Symbol already subscribed", _type.FullName, "SubscribeTickData");
                    }

                    return false;
                }

                // Stop comsuming current data
                _consumeQuoteStream = false;

                // Reset Consumer
                StopComsumer();
                StartDataConsumer();

                Thread.Sleep(50);

                // Add request to queue
                _symbolSubscriptionCollection.Add(subscribe);

                return true;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "SubscribeTickData");
                return false;
            }
        }

        /// <summary>
        /// Unsubscribe Market data message
        /// </summary>
        public bool UnsubscribeTickData(Unsubscribe unsubscribe)
        {
            try
            {
                // Saftey check to avoid duplication
                if (!_subscribedSymbols.Contains(unsubscribe.Security.Symbol))
                {
                    if (_logger.IsInfoEnabled)
                    {
                        _logger.Info("Symbol is not subscribed", _type.FullName, "UnsubscribeTickData");
                    }

                    return false;
                }

                // Stop comsuming current data
                _consumeQuoteStream = false;

                // Remove from subscribed symbols map
                _subscribedSymbols.Remove(unsubscribe.Security.Symbol);

                if (_subscribedSymbols.Count != 0)
                {
                    // Create a new subscription message with empty symbol
                    // This will allow the remaining symbols to be subscribed 
                    // While the unsubcribed symbol is removed from the stream request
                    var subscribe = new Subscribe() {Security = new Security() {Symbol = String.Empty}};

                    // Add request to queue
                    _symbolSubscriptionCollection.Add(subscribe);
                }

                if (_logger.IsInfoEnabled)
                {
                    _logger.Info(unsubscribe.Security.Symbol + " unsubscribed", _type.FullName, "UnsubscribeTickData");
                }

                return true;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "UnsubscribeTickData");
                return false;
            }
        }

        /// <summary>
        /// Historic Bar Data Request Message
        /// </summary>
        public bool HistoricBarDataRequest(HistoricDataRequest historicDataRequest)
        {
            try
            {
                // Create Query Properties
                string symbol = "symbol=" + historicDataRequest.Security.Symbol;
                string interval = "interval=" + historicDataRequest.BarType.ToLowerInvariant();
                string start = "start=" + historicDataRequest.StartTime.ToString("yyyy-MM-dd");
                string end = "end=" + historicDataRequest.EndTime.ToString("yyyy-MM-dd");
                
                // Create complete HTTP Url
                string url = _credentials.ApiUrl + "/markets/history?" + symbol + "&" + interval + "&" + start + "&" + end;
            
                // Create request message
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                request.AddHeader("authorization", "Bearer " + _credentials.AccessToken);
                request.AddHeader("accept", "application/json");

                // Send Request
                IRestResponse response = client.Execute(request);

                // Parse JSON object
                var responseResult = JsonConvert.DeserializeObject<TradierHistoricalData>(response.Content);

                // Create TradeHub Historic Bar Data Object
                var historicBarData = CreateHistoricalBarData(responseResult, historicDataRequest.Security, historicDataRequest.Id);

                if (historicBarData!=null)
                {
                    // Raise event to notify listeners
                    if (HistoricBarDataArrived!=null)
                    {
                        HistoricBarDataArrived(historicBarData);
                    }
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "HistoricBarDataRequest");
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Reads quotes from stream session established with the server
        /// </summary>
        private void ConsumeQuoteStream()
        {
            try
            {
                while (true)
                {
                    // Retrive new subscription
                    var subscribe = _symbolSubscriptionCollection.Take();

                    // Get session details for requesting Quote Stream
                    GetStreamSessionDetails();

                    if (_tradierStreamSession == null)
                    {
                        if (_logger.IsInfoEnabled)
                        {
                            _logger.Info("Stream Session details not found", _type.FullName, "ConsumeQuoteStream");
                        }

                        break;
                    }

                    // Construct query string to be used with request message
                    string queryString = CreateQuotesQueryString(subscribe);

                    // Create request message
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_tradierStreamSession.Url + queryString);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Accept = "application/json";

                    // Get response message
                    HttpWebResponse response = (HttpWebResponse) request.GetResponse();

                    // Extract stream from incoming response message
                    StreamReader readStream = new StreamReader(response.GetResponseStream(), Encoding.UTF8);

                    _consumeQuoteStream = true;

                    // Add symbol to local collection
                    _subscribedSymbols.Add(subscribe.Security.Symbol);

                    // Read individual data from extracted stream
                    string streamLine = readStream.ReadLine();
                    while (_consumeQuoteStream)
                    {
                        if (streamLine != null)
                        {
                            // Exctract information from JSON object
                            var streamResult = JsonConvert.DeserializeObject<QuoteStream>(streamLine);

                            // Construct Tick object
                            var tick = CreateTickData(streamResult);

                            if (tick != null)
                            {
                                // Raise Event to notify listeners
                                if (TickArrived != null)
                                {
                                    TickArrived(tick);
                                }
                            }
                        }

                        // Read next line of data
                        streamLine = readStream.ReadLine();
                    }

                    readStream.Close();
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "ConsumeQuoteStream");
                StopComsumer();
            }
        }

        /// <summary>
        /// Create Query String for requesting Quote Stream
        /// </summary>
        /// <returns></returns>
        private string CreateQuotesQueryString(Subscribe subscribe)
        {
            int count = 0;
            int subscribedSymbolsCount = _subscribedSymbols.Count;

            // Create Query parameters
            string sessionId = "sessionid=" + _tradierStreamSession.Sessionid;

            string symbols = "symbols=";

            if (subscribe.Security.Symbol!=String.Empty)
            {
                symbols += subscribe.Security.Symbol;
            }

            // Traverse all currently subscribed symbols
            foreach (string subscribedSymbol in _subscribedSymbols)
            {
                //if (++count != subscribedSymbolsCount)
                {
                    symbols += ",";
                }

                symbols += subscribedSymbol;

            }

            //symbols += ",CAKE,MSFT,NFLX";

            if (subscribe.Security.Symbol.Equals(""))
            {
                symbols = symbols.Remove(0, 1);
            }

            string linebreak = "linebreak=true";

            // Combine all parameters and return
            return  "?" + sessionId + "&" + symbols + "&" + linebreak;
        }

        /// <summary>
        /// Gets session details to be used for streaming quotes
        /// </summary>
        /// <returns></returns>
        private void GetStreamSessionDetails()
        {
            try
            {
                // Verify if the existing session is valid or not
                if (CheckStreamSessionValidity())
                {
                    return;
                }

                // Create request message
                var client = new RestClient(_credentials.ApiUrl + "/markets/events/session");
                var request = new RestRequest(Method.POST);
                request.AddHeader("authorization", "Bearer " + _credentials.AccessToken);
                request.AddHeader("accept", "application/json");

                // Send request
                IRestResponse response = client.Execute(request);

                // Parse received JSON message
                var responseResult = JsonConvert.DeserializeObject<dynamic>(response.Content);

                string sessionId = responseResult.stream.sessionid.ToString();
                string sessionUrl = responseResult.stream.url.ToString();

                if (!String.IsNullOrEmpty(sessionId) && !String.IsNullOrEmpty(sessionUrl))
                {
                    _tradierStreamSession = new TradierStreamSession();

                    // Copy session details
                    _tradierStreamSession.Sessionid = sessionId;
                    _tradierStreamSession.Url = sessionUrl;
                    _tradierStreamSession.CreationTime = DateTime.Now.AddMinutes(5);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "GetStreamSessionDetails");
            }
        }

        /// <summary>
        /// Verifies if the Stream Session is still valid or not
        /// </summary>
        /// <returns></returns>
        private bool CheckStreamSessionValidity()
        {
            if (_tradierStreamSession != null && (_tradierStreamSession.CreationTime < DateTime.Now))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a new TradeHub Tick Data object
        /// </summary>
        /// <param name="quoteStream"></param>
        /// <returns></returns>
        private Tick CreateTickData(QuoteStream quoteStream)
        {
            try
            {
                if (quoteStream.type.Equals("summary"))
                {
                    return null;
                }
                // Format our new DateTime object to start at the UNIX Epoch
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);

                // Create new Tick object
                Tick tick = new Tick(new Security() { Symbol = quoteStream.symbol }, _marketDataProviderName);

                if (quoteStream.type.Equals("quote"))
                {
                    // Extract UNIX timestamp
                    var timestamp = Convert.ToDouble(quoteStream.biddate);

                    // Add the timestamp (number of seconds since the Epoch) to be converted
                    dateTime = dateTime.AddMilliseconds(timestamp);

                    // Set time to local object
                    tick.DateTime = dateTime;

                    // Extract BID information
                    tick.BidPrice = Convert.ToDecimal(quoteStream.bid);
                    tick.BidSize = Convert.ToDecimal(quoteStream.bidsz);
                    tick.BidExchange = quoteStream.bidexch;

                    // Extract ASK information
                    tick.AskPrice = Convert.ToDecimal(quoteStream.ask);
                    tick.AskSize = Convert.ToDecimal(quoteStream.asksz);
                    tick.AskExchange = quoteStream.askexch;
                }
                else if (quoteStream.type.Equals("trade"))
                {
                    // Extract UNIX timestamp
                    var timestamp = Convert.ToDouble(quoteStream.date);

                    // Add the timestamp (number of seconds since the Epoch) to be converted
                    dateTime = dateTime.AddMilliseconds(timestamp);

                    // Set time to local object
                    tick.DateTime = dateTime;

                    // Extract LAST/TRADE information
                    tick.LastPrice = Convert.ToDecimal(quoteStream.price);
                    tick.LastSize = Convert.ToDecimal(quoteStream.size);
                    tick.LastExchange = quoteStream.exch;
                }

                return tick;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "CreateTickData");
                return null;
            }
        }

        /// <summary>
        /// Create TradeHub Historical Bar Data object from the Tradier data
        /// </summary>
        /// <param name="tradierHistoricalData"></param>
        /// <param name="security"></param>
        /// <param name="requestId"></param>
        private HistoricBarData CreateHistoricalBarData(TradierHistoricalData tradierHistoricalData, Security security, string requestId)
        {
            try
            {
                // Create new TradeHub Historical Bar Data object to be used withing the application
                HistoricBarData historicBarData = new HistoricBarData(security, _marketDataProviderName, DateTime.UtcNow);

                Bar[] bars = new Bar[tradierHistoricalData.history.day.Count];
                int count = 0;

                foreach (var historicalDataDetail in tradierHistoricalData.history.day)
                {
                    var time = DateTime.ParseExact(historicalDataDetail.date, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                    // Create new TradeHub Bar
                    Bar bar = new Bar(security, _marketDataProviderName, requestId, time)
                    {
                        Close = Convert.ToDecimal(historicalDataDetail.close),
                        Open = Convert.ToDecimal(historicalDataDetail.open),
                        High = Convert.ToDecimal(historicalDataDetail.high),
                        Low = Convert.ToDecimal(historicalDataDetail.low),
                        Volume = historicalDataDetail.volume
                    };

                    // Add to the Array
                    bars[count] = bar;
                    count++;
                }

                // Add Bars array to root object
                historicBarData.Bars = bars;

                // Add Request ID to root object
                historicBarData.ReqId = requestId;

                return historicBarData;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "CreateHistoricalBarData");
                return null;
            }
        }

        /// <summary>
        /// Starts process to consume streaming quote data
        /// </summary>
        private void StartDataConsumer()
        {
            if (_quotesConsumerCancellationToken == null)
            {
                _symbolSubscriptionQueue = new ConcurrentQueue<Subscribe>();
                _symbolSubscriptionCollection = new BlockingCollection<Subscribe>(_symbolSubscriptionQueue);

                // Initialize Consumer Token
                _quotesConsumerCancellationToken = new CancellationTokenSource();

                // Start consumer for quotes from local collection
                _quotesConsumerTask = Task.Factory.StartNew(ConsumeQuoteStream, _quotesConsumerCancellationToken.Token);
            }
        }

        /// <summary>
        /// Stops data comsumer process
        /// </summary>
        private void StopComsumer()
        {
            if (_quotesConsumerCancellationToken != null)
            {
                _consumeQuoteStream = false;
                _quotesConsumerCancellationToken.Cancel();

                _quotesConsumerTask = null;
                _quotesConsumerCancellationToken = null;

                // Stop data comsumer
                _symbolSubscriptionCollection.Dispose();
                _symbolSubscriptionCollection = null;
                _symbolSubscriptionQueue = null;
            }
        }
    }
}
