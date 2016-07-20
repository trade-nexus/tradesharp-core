using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.Common.HistoricalDataProvider.Utility;
using TradeHub.Common.HistoricalDataProvider.ValueObjects;

namespace TradeHub.Common.HistoricalDataProvider.Service
{
    /// <summary>
    /// Responsible for managing all Market Data requests and providing appropriate responses
    /// </summary>
    public class DataHandler : IEventHandler<MarketDataObject>
    {
        private Type _type = typeof(DataHandler);
        private AsyncClassLogger _classLogger;

        public event Action<Tick> TickReceived;
        public event Action<Bar> BarReceived;
        public event Action<HistoricBarData> HistoricDataReceived;

        private int _persistanceDataCount = 0;

        private readonly bool _localPersistance;
        private bool _disposed = false;

        private FetchData _fetchMarketData;

        /// <summary>
        /// Will contain all the symbols for which bar is subscribed
        /// </summary>
        private IList<string> _barSubscriptionList;

        /// <summary>
        /// Will contain all the symbols for which tick is subscribed
        /// </summary>
        private IList<string> _tickSubscriptionList;

        /// <summary>
        /// Contains all tasks started by Handler
        /// </summary>
        private ConcurrentBag<Task> _tasksCollection;

        /// <summary>
        /// Saves the data locally for future use
        /// </summary>
        private SortedDictionary<int, MarketDataObject> _localPersistanceData;

        public bool ConnectionStatus { get; set; }

        /// <summary>
        /// Will contain all the symbols for which bar is subscribed
        /// </summary>
        public IList<string> BarSubscriptionList
        {
            get { return _barSubscriptionList; }
            set { _barSubscriptionList = value; }
        }

        /// <summary>
        /// Will contain all the symbols for which tick is subscribed
        /// </summary>
        public IList<string> TickSubscriptionList
        {
            get { return _tickSubscriptionList; }
            set { _tickSubscriptionList = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DataHandler(bool localPersistance = false)
        {
            try
            {
                _classLogger = new AsyncClassLogger("SimulatedDataHandler");
                // Set logging level
                _classLogger.SetLoggingLevel();
                //set logging path
                _classLogger.LogDirectory(DirectoryStructure.CLIENT_LOGS_LOCATION);

                _persistanceDataCount = 0;
                _localPersistance = localPersistance;
                _localPersistanceData = new SortedDictionary<int, MarketDataObject>();
                _tasksCollection = new ConcurrentBag<Task>();
                
                _fetchMarketData = new FetchData(new ReadMarketData(_classLogger), _classLogger);

                // Initialize Lists
                BarSubscriptionList = new List<string>();
                TickSubscriptionList = new List<string>();

                _fetchMarketData.InitializeDisruptor(new IEventHandler<MarketDataObject>[] { this });
                _fetchMarketData.HistoricalDataFired += HistoricDataArrived;
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "DataHandler");
            }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="eventHandler">Event handler which will receive requested data</param>
        /// <param name="localPersistance">Indicates whether the data to be saved locally on initial request</param>
        public DataHandler(IEventHandler<MarketDataObject>[] eventHandler, bool localPersistance=false)
        {
            try
            {
                _classLogger = new AsyncClassLogger("SimulatedDataHandler");
                // Set logging level
                _classLogger.SetLoggingLevel();
                //set logging path
                _classLogger.LogDirectory(DirectoryStructure.CLIENT_LOGS_LOCATION);
                
                _persistanceDataCount = 0;
                _localPersistance = localPersistance;
                _localPersistanceData = new SortedDictionary<int, MarketDataObject>();
                _tasksCollection = new ConcurrentBag<Task>();

                _fetchMarketData = new FetchData(new ReadMarketData(_classLogger), _classLogger);

                // Initialize Lists
                BarSubscriptionList = new List<string>();
                TickSubscriptionList = new List<string>();

                _fetchMarketData.InitializeDisruptor(eventHandler);
                _fetchMarketData.HistoricalDataFired += HistoricDataArrived;
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "DataHandler");
            }
        }

        #region Subscription messages

        /// <summary>
        /// Subscribe to New Symbol for bar data
        /// </summary>
        /// <param name="barDataRequest"></param>
        public void SubscribeSymbol(BarDataRequest barDataRequest)
        {
            if (_classLogger.IsInfoEnabled)
            {
                _classLogger.Info("New subscription request recieved Request for " + barDataRequest, _type.FullName,
                                  "SubscribeSymbol");
            }

            // Add new symbol to the Bar list
            if (!BarSubscriptionList.Contains(barDataRequest.Security.Symbol))
            {
                BarSubscriptionList.Add(barDataRequest.Security.Symbol);
            }

            // Fetch data if its not already fetched for ticks
            if (!TickSubscriptionList.Contains(barDataRequest.Security.Symbol))
            {
                // Use locally saved data
                if (_persistanceDataCount > 0)
                {
                    var task = Task.Factory.StartNew(UseLocalData);

                    _tasksCollection.Add(task);
                }
                // Fetch fresh data
                else
                {
                    FetchData(barDataRequest);
                }
            }
        }

        /// <summary>
        /// Subscribe to New Symbol for bar data
        /// </summary>
        /// <param name="barDataRequest"></param>
        public void SubscribeMultiSymbol(BarDataRequest[] barDataRequest)
        {
            if (_classLogger.IsInfoEnabled)
            {
                _classLogger.Info("New subscription request recieved Request for " + barDataRequest, _type.FullName,
                                  "SubscribeSymbol");
            }
            for (int i = 0; i < barDataRequest.Length; i++)
            {
                // Add new symbol to the Bar list
                if (!BarSubscriptionList.Contains(barDataRequest[i].Security.Symbol))
                {
                    BarSubscriptionList.Add(barDataRequest[i].Security.Symbol);
                }
            }
            FetchData(barDataRequest);
        }
        /// <summary>
        /// Subscribes Tick data for the given symbol
        /// </summary>
        /// <param name="subscribe">Contains info for the symbol to be subscribed</param>
        public void SubscribeSymbol(Subscribe subscribe)
        {
            try
            {
                if (_classLogger.IsInfoEnabled)
                {
                    _classLogger.Info("New subscription request received " + subscribe, _type.FullName,
                                      "SubscribeSymbol");
                }

                // Add new symbol to the Tick list
                if (!TickSubscriptionList.Contains(subscribe.Security.Symbol))
                {
                    TickSubscriptionList.Add(subscribe.Security.Symbol);
                }

                // Fetch data if its not already fetched for bars
                if (!BarSubscriptionList.Contains(subscribe.Security.Symbol))
                {
                    // Use locally saved data
                    if (_persistanceDataCount > 0)
                    {
                        var task = Task.Factory.StartNew(UseLocalData);

                        _tasksCollection.Add(task);
                    }
                    // Fetch fresh data
                    else
                    {
                        FetchData(subscribe);
                    }
                }
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "SubscribeSymbol");
            }
        }

        /// <summary>
        /// Subscribe to New Symbol for Historical data
        /// </summary>
        /// <param name="historicDataRequest"></param>
        public void SubscribeSymbol(HistoricDataRequest historicDataRequest)
        {
            if (_classLogger.IsInfoEnabled)
            {
                _classLogger.Info("New subscription request received for " + historicDataRequest, _type.FullName,
                                  "SubscribeSymbol");
            }
            FetchData(historicDataRequest);
        }

        /// <summary>
        /// Unsubscribes the given symbol for Tick data
        /// </summary>
        /// <param name="unsubscribe"></param>
        public void UnsubscribleSymbol(Unsubscribe unsubscribe)
        {
            try
            {
                if (_classLogger.IsInfoEnabled)
                {
                    _classLogger.Info("Unsubscription request received " + unsubscribe, _type.FullName,
                                      "UnsubscribleSymbol");
                }

                // Remove symbol from the Tick list
                TickSubscriptionList.Remove(unsubscribe.Security.Symbol);
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "SubscribeSymbol");
            }
        }

        /// <summary>
        /// Unsubscribes the given symbol for Bar data
        /// </summary>
        /// <param name="barDataRequest"></param>
        public void UnsubscribleSymbol(BarDataRequest barDataRequest)
        {
            if (_classLogger.IsInfoEnabled)
            {
                _classLogger.Info("Unsubscription request received " + barDataRequest, _type.FullName,
                                  "UnsubscribleSymbol");
            }

            // Remove symbol from the Bar list
            BarSubscriptionList.Remove(barDataRequest.Security.Symbol);
        }

        #endregion

        /// <summary>
        /// Fire Historical Bar Data
        /// </summary>
        /// <param name="historicBarData">TradeHub HistoricalBarData contains requested historical bars</param>
        private void HistoricDataArrived(HistoricBarData historicBarData)
        {
            try
            {
                if (HistoricDataReceived != null)
                {
                    HistoricDataReceived(historicBarData);
                }

                if (_classLogger.IsDebugEnabled)
                {
                    _classLogger.Debug(historicBarData.ToString(), _type.FullName, "HistoricDataArrived");
                }
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "HistoricDataArrived");
            }
        }

        /// <summary>
        /// Creats a seprate thread for each request.
        /// </summary>
        /// <param name="request"></param>
        public void FetchData(BarDataRequest request)
        {
            try
            {
                var task = Task.Factory.StartNew(() => _fetchMarketData.ReadData(request));

                _tasksCollection.Add(task);
                
                //_marketDataList=_fetchMarketData.ReadData(request,true);
                //_persistanceDataCount=_marketDataList.Count;
                //Task.Factory.StartNew(UseLocalData1);
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "FetchData");
            }
        }

        /// <summary>
        /// Create thread for multi request.
        /// </summary>
        /// <param name="request"></param>
        public void FetchData(BarDataRequest[] request)
        {
            try
            {
                var task = Task.Factory.StartNew(() => _fetchMarketData.MultiSymbolReadData(request));

                _tasksCollection.Add(task);

                //_marketDataList=_fetchMarketData.ReadData(request,true);
                //_persistanceDataCount=_marketDataList.Count;
                //Task.Factory.StartNew(UseLocalData1);
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "FetchData");
            }
        }

        /// <summary>
        /// Fetches data for the required symbol from stored files
        /// </summary>
        /// <param name="subscribe">Contains info for the subscribing symbol</param>
        private void FetchData(Subscribe subscribe)
        {
            try
            {
                var task = Task.Factory.StartNew(() => _fetchMarketData.ReadData(subscribe));

                _tasksCollection.Add(task);
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "FetchData");
            }
        }

        /// <summary>
        /// Fetches data for the required symbol from stored files
        /// </summary>
        /// <param name="historicDataRequest">Contains historical request info for subscribing symbol</param>
        private void FetchData(HistoricDataRequest historicDataRequest)
        {
            try
            {
                var task = Task.Factory.StartNew(() => _fetchMarketData.ReadData(historicDataRequest));

                _tasksCollection.Add(task);
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "FetchData");
            }
        }

        /// <summary>
        /// Provides locally saved data to the requesting party
        /// </summary>
        private void UseLocalData()
        {
            foreach (KeyValuePair<int, MarketDataObject> marketDataObject in _localPersistanceData)
            {
                if (marketDataObject.Value.IsTick)
                {
                    if (TickReceived != null)
                    {
                        TickReceived(marketDataObject.Value.Tick);
                    }
                }
                else
                {
                    if (BarReceived != null)
                    {
                        BarReceived(marketDataObject.Value.Bar);
                    }
                }
            }
        }

        /// <summary>
        /// Called when Bar/Tick data is completely sent
        /// </summary>
        /// <param name="message"></param>
        private void OnDataCompleted(string message)
        {
            try
            {
                // NOTE: Commented out because Disruptor is still sending data while this event is raised.
                if (message.Contains("DataCompleted"))
                {
                    //var info = message.Split(',');

                    //if (_barSubscriptionList.Contains(info[1]))
                    //    _barSubscriptionList.Remove(info[1]);

                    //if (_tickSubscriptionList.Contains(info[1]))
                    //    _tickSubscriptionList.Remove(info[1]);
                }
            }
            catch (Exception exception)
            {
                _classLogger.Error(exception, _type.FullName, "OnDataCompleted");
            }
        }

        /// <summary>
        /// Clears local maps
        /// </summary>
        public void ClearMaps()
        {
            _tickSubscriptionList.Clear();
            _barSubscriptionList.Clear();
            _localPersistanceData.Clear();
        }

        /// <summary>
        /// Close active connections/services
        /// </summary>
        public void Shutdown()
        {
            ClearMaps();
            _fetchMarketData.ShutdownDisruptor();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    DisposeTasks();
                    ClearMaps();
                    _fetchMarketData.Dispose();
                }
                // Release unmanaged resources.
                _fetchMarketData = null;
                _tickSubscriptionList = null;
                _barSubscriptionList = null;
                _localPersistanceData = null;
                _tasksCollection = null;
                _disposed = true;
            }
        }

        /// <summary>
        /// Dispose all existing tasks
        /// </summary>
        private void DisposeTasks()
        {
            foreach (var task in _tasksCollection)
            {
                task.Dispose();
            }
        }

        #region Implementation of IEventHandler<in MarketDataObject>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(MarketDataObject data, long sequence, bool endOfBatch)
        {
            if (data.IsTick)
            {
                // Publish Tick if the subscription request is received
                if (TickSubscriptionList.Contains(data.Tick.Security.Symbol))
                {
                    // Raise Event to notify listeners
                    if (TickReceived != null)
                        TickReceived(data.Tick);
                }
            }
            else
            {
                // Publish Bar if the subscription request is received
                if (BarSubscriptionList.Contains(data.Bar.Security.Symbol))
                {
                    // Raise Event to notify listeners
                    if (BarReceived != null)
                        BarReceived(data.Bar);
                }
            }

            // Save data locally for future use
            if(_localPersistance)
            {
                _localPersistanceData.Add(_persistanceDataCount, data);
                _persistanceDataCount++;
            }
        }

        #endregion
    }
}
