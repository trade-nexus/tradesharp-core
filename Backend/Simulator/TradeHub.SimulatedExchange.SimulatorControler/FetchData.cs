using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Disruptor;
using Disruptor.Dsl;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.SimulatedExchange.Common;
using TradeHub.SimulatedExchange.Common.Interfaces;
using TradeHub.SimulatedExchange.Common.ValueObjects;

namespace TradeHub.SimulatedExchange.SimulatorControler
{
    public class FetchData: IFetchData
    {
        private Type _type = typeof(FetchData);
        public event Action<Bar,string> BarFired;
        public event Action<Tick> TickFired;
        public event Action<HistoricBarData> HistoricalDataFired;
        private IReadMarketData _readMarketData;
        private DateTime _startDate = new DateTime(2013, 08, 06);
        private DateTime _endDate = new DateTime(2013, 08, 07);
        private string _providerName = MarketDataProvider.InteractiveBrokers;

        private MarketDataControler _marketDataControler;
        private SimulatedOrderController _simulatedOrderController;

        private readonly int _ringSize = 1048576;//262144;//65536;  // Must be multiple of 2

        private Disruptor<MarketDataObject> _disruptor;
        private RingBuffer<MarketDataObject> _ringBuffer;
        private EventPublisher<MarketDataObject> _publisher;

        public MarketDataControler MarketDataControler
        {
            get { return _marketDataControler; }
            set
            {
                _marketDataControler = value;
                InitializeDisruptor();
            }
        }

        public SimulatedOrderController SimulatedOrderController
        {
            get { return _simulatedOrderController; }
            set
            {
                _simulatedOrderController = value;
                //InitializeDisruptor();
            }
        }

        /// <summary>
        /// Constructor:
        /// Loads Setting From Xml File
        /// </summary>
        /// <param name="readMarketData"></param>
        public FetchData(IReadMarketData readMarketData)
        {
            try
            {
                _readMarketData = readMarketData;

                //Reading Settings From Xml File
                XDocument doc = XDocument.Load("SimulatedExchangeConfiguration\\SimulatedExchangeSetting.xml");
                var startDate = doc.Descendants("StartDate");
                foreach (var xElement in startDate)
                {
                    string[] start = xElement.Value.Split(',');
                    _startDate = new DateTime(Convert.ToInt32(start[0]), Convert.ToInt32(start[1]), Convert.ToInt32(start[2]));
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("StartDate:" + _startDate.ToString(CultureInfo.InvariantCulture), _type.FullName, "FetchData");
                    }
                }

                var endDate = doc.Descendants("EndDate");
                foreach (var xElement in endDate)
                {
                    string[] end = xElement.Value.Split(',');
                    _endDate = new DateTime(Convert.ToInt32(end[0]), Convert.ToInt32(end[1]), Convert.ToInt32(end[2]));
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("EndDate:" + _endDate.ToString(CultureInfo.InvariantCulture), _type.FullName, "FetchData");
                    }
                }

                var provider = doc.Descendants("Provider");
                foreach (var xElement in provider)
                {
                    _providerName = xElement.Value;
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("ProviderName:" + _providerName.ToString(CultureInfo.InvariantCulture), _type.FullName, "FetchData");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "FetchData");
            }
        }

        /// <summary>
        /// Reading Data From ReadMarketData class.
        /// </summary>
        /// <param name="request"></param>
        public virtual void ReadData(BarDataRequest request)
        {
            try
            {
                IEnumerable<Bar> barlist = _readMarketData.ReadBars(_startDate, _endDate, _providerName, request);

                #region Send Required Info

                foreach (var bar in barlist)
                {
                    // Update time
                    DateTime time = bar.DateTime.AddMinutes(-1);

                    for (int i = 0; i < 4; i++)
                    {
                        //Create Object to be disptched
                        MarketDataObject marketDataObjectTick = new MarketDataObject();

                        // Create a new tick object
                        Tick tick = new Tick(bar.Security, MarketDataProvider.SimulatedExchange)
                            {
                                // Add Last Price to new Tick instance
                                LastPrice = GetRequiredPrice(i, bar),
                                // Add Size
                                LastSize = 100,
                                // Set updated time
                                DateTime = time.AddSeconds((i + 1)*14)
                            };

                        // Add Values to the object to be dispatched
                        marketDataObjectTick.IsTick = true;
                        marketDataObjectTick.Tick = tick;

                        // Raise event to notify listeners
                        _publisher.PublishEvent((entry, sequenceNo) =>
                        {
                            entry.IsTick = marketDataObjectTick.IsTick;
                            entry.Tick = marketDataObjectTick.Tick;
                            return entry;
                        });

                        //if (TickFired != null)
                        //{
                        //    TickFired.Invoke(tick);
                        //}
                    }

                    //Create Object to be disptched
                    MarketDataObject marketDataObjectBar = new MarketDataObject();
                    marketDataObjectBar.Bar = bar;

                    _publisher.PublishEvent((entry, sequenceNo) =>
                    {
                        entry.Bar = marketDataObjectBar.Bar;
                        return entry;
                    });

                    //if (BarFired != null)
                    //{
                    //    BarFired.Invoke(bar, request.Id);
                    //}
                }

                #endregion

                //EventSystem.Publish<string>("DataCompleted," + request.Security.Symbol);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadData");
            }
        }

        /// <summary>
        /// Reads data for required symbol from stored files
        /// </summary>
        /// <param name="subscribe">Contains Symbol info</param>
        public virtual void ReadData(Subscribe subscribe)
        {
            try
            {
                // Get all available Bars
                var barlist = _readMarketData.ReadBars(_startDate, _endDate, _providerName, subscribe);

                foreach (var bar in barlist)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        // Create a new tick object
                        Tick tick = new Tick(bar.Security, MarketDataProvider.SimulatedExchange, bar.DateTime);

                        // Add Last Price to new Tick instance
                        if (GetRequiredPrice(i, bar) != default(decimal))
                        {
                            // Add Price
                            tick.LastPrice = GetRequiredPrice(i, bar);

                            // Add Size
                            tick.LastSize = 100;

                            // Update time
                            DateTime time = bar.DateTime.AddMinutes(-1);

                            // Set updated time
                            tick.DateTime = time.AddSeconds((i + 1)*14);

                            // Raise event to notify listeners
                            if (TickFired != null)
                            {
                                TickFired.Invoke(tick);
                            }
                        }
                    }

                    bar.MarketDataProvider = MarketDataProvider.SimulatedExchange;
                    bar.RequestId = "";
                    if (BarFired != null)
                    {
                        BarFired.Invoke(bar, "");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadData");
            }
        }

        /// <summary>
        /// Reads data for required symbol from stored files
        /// </summary>
        /// <param name="historicDataRequest">Contains historical request info for subscribing symbol</param>
        public virtual void ReadData(HistoricDataRequest historicDataRequest)
        {
            try
            {
                // Get all available Bars
                var barlist = _readMarketData.ReadBars(_providerName, historicDataRequest);

                // Create new historic bar data response object
                HistoricBarData historicBarData = new HistoricBarData(historicDataRequest.Security, MarketDataProvider.SimulatedExchange, DateTime.Now);
                
                // Add Bars
                historicBarData.Bars = barlist.ToArray();
                // Add Request ID
                historicBarData.ReqId = historicDataRequest.Id;

                // Raise event to notify listeners
                if (HistoricalDataFired != null)
                {
                    HistoricalDataFired(historicBarData);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadData");
            }
        }

        /// <summary>
        /// Initialize Disruptor
        /// </summary>
        private void InitializeDisruptor()
        {
            if (_disruptor == null)
            {
                //if (_marketDataControler != null && _simulatedOrderController != null)
                //if (_marketDataControler != null)
                {
                    // Initialize Disruptor
                    _disruptor = new Disruptor<MarketDataObject>(() => new MarketDataObject(), _ringSize,
                                                                 TaskScheduler.Default);
                    // Add Consumer
                    //_disruptor.HandleEventsWith(_marketDataControler).Then(_simulatedOrderController);
                    _disruptor.HandleEventsWith(_marketDataControler);
                    // Start Disruptor
                    _ringBuffer = _disruptor.Start();
                    // Get Publisher
                    _publisher = new EventPublisher<MarketDataObject>(_ringBuffer);
                }
            }
        }

        /// <summary>
        /// Provides required price type from the given bar
        /// </summary>
        /// <param name="iteration">Iteration count</param>
        /// <param name="bar">Bar to process</param>
        /// <returns></returns>
        private decimal GetRequiredPrice(int iteration, Bar bar)
        {
            switch (iteration)
            {
                case 0:
                    return bar.Open;
                case 1:
                    return bar.High;
                case 2:
                    return bar.Low;
                case 3:
                    return bar.Close;
                default:
                    return default(decimal);
            }
        }
    }
}
