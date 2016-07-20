using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.SimulatedExchange.Common;
using TradeHub.SimulatedExchange.Common.Interfaces;
using TradeHub.SimulatedExchange.Common.ValueObjects;
using TradeHub.SimulatedExchange.DomainObjects.Constant;


namespace TradeHub.SimulatedExchange.SimulatorControler
{
    public class SimulatedOrderController : IEventHandler<MarketDataObject>, IEventHandler<Tick>
    {
        private Type _type = typeof (SimulatedOrderController);

        private ConcurrentQueue<Bar> _barsQueue;
        private ConcurrentQueue<Tick> _ticksQueue;
        private BlockingCollection<Bar> _blockingCollectionBarData;
        private BlockingCollection<Tick> _blockingCollectionTickData;

        private ICommunicationController _communicationController;
        private SimulateMarketOrder _simulateMarketOrder;
        private SimulateLimitOrder _simulateLimitOrder;
        
        // Used to stop reading bar data if tick data is available
        private static int _tickDataCount = 0;

        private readonly int _ringSize = 262144; //65536;  // Must be multiple of 2

        private Disruptor<Tick> _tickDisruptor;
        private RingBuffer<Tick> _tickRingBuffer;

        /// <summary>
        /// Constructor:
        /// Hooks all the required Events of SimulateMarketOrder class and SimulateLimitOrderClass
        /// </summary>
        /// <param name="communicationController"> </param>
        /// <param name="simulateMarketOrder"></param>
        /// <param name="simulateLimitOrder"> </param>
        public SimulatedOrderController(ICommunicationController communicationController, SimulateMarketOrder simulateMarketOrder, SimulateLimitOrder simulateLimitOrder)
        {
            try
            {
                // Initialize Disruptor
                _tickDisruptor = new Disruptor.Dsl.Disruptor<Tick>(() => new Tick(), _ringSize, TaskScheduler.Default);
                // Add Consumer
                _tickDisruptor.HandleEventsWith(this);
                // Start Disruptor
                _tickRingBuffer = _tickDisruptor.Start();

                _communicationController = communicationController;
                _simulateLimitOrder = simulateLimitOrder;
                _simulateMarketOrder = simulateMarketOrder;

                //Initializing  ConcurrentQueue for bar.
                _barsQueue = new ConcurrentQueue<Bar>();
                _ticksQueue = new ConcurrentQueue<Tick>();
                _blockingCollectionBarData = new BlockingCollection<Bar>(_barsQueue);
                _blockingCollectionTickData = new BlockingCollection<Tick>(_ticksQueue);

                _simulateLimitOrder.LimitOrderRejection += RejectionArrived;
                _simulateMarketOrder.MarketOrderRejection += RejectionArrived;
                _simulateLimitOrder.NewArrived += SimulateLimitOrderNewArrived;
                _simulateLimitOrder.NewExecution += SimulateLimitOrderNewExecution;
                _simulateMarketOrder.NewArrived += SimulateMarketOrderNewArrived;
                _simulateMarketOrder.NewExecution += SimulateMarketOrderNewExecution;

                EventSystem.Subscribe<Bar>(NewBarArrived);
                EventSystem.Subscribe<Tick>(NewTickArrived);
                EventSystem.Subscribe<int>(SetTickDataCount);

                HookAllRequiredEvents();
                //Task.Factory.StartNew(ReadBarFromQueue);
                //Task.Factory.StartNew(ReadTickFromQueue);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SimulatedOrderController");
            }
        }

        /// <summary>
        /// Limit Order Execution Arrived
        /// </summary>
        /// <param name="execution"></param>
        private void SimulateLimitOrderNewExecution(Execution execution)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info(execution.ToString(), _type.FullName, "SimulateLimitOrderNewExecution");
            }
            try
            {
                _communicationController.PublishExecutionOrder(execution);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SimulateLimitOrderNewExecution");
            }
        }

        /// <summary>
        /// Limit Order New Arrived
        /// </summary>
        /// <param name="order"></param>
        private void SimulateLimitOrderNewArrived(Order order)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info(order.ToString(), _type.FullName, "SimulateLimitOrderNewArrived");
            }
            try
            {
                _communicationController.PublishNewOrder(order);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SimulateLimitOrderNewArrived");
            }
        }

        /// <summary>
        /// Rejection Arrived
        /// </summary>
        /// <param name="rejection"></param>
        private void RejectionArrived(Rejection rejection)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Sending Rejection To OEE", _type.FullName, "RejectionArrived");
            }
            try
            {
                _communicationController.PublishOrderRejection(rejection);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RejectionArrived");
            }
        }

        /// <summary>
        /// New Execution Arrived From SimulatedMarketOrder Class
        /// </summary>
        /// <param name="newExecution"> </param>
        private void SimulateMarketOrderNewExecution(Execution newExecution)
        {
            // Publish Execution
            _communicationController.PublishExecutionOrder(newExecution);

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("New Execution Arrived " + newExecution, _type.FullName, "SimulateMarketOrderNewExecution");
            }
        }

        /// <summary>
        /// NewArrived Message from Simulated Market Order class. 
        /// </summary>
        /// <param name="order"></param>
        private void SimulateMarketOrderNewArrived(Order order)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info(order.ToString(), _type.FullName, "SimulateMarketOrderNewArrived");
            }
            try
            {
                // Publish Order acceptance
                _communicationController.PublishNewOrder(order);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SimulateMarketOrderNewArrived");
            }
        }

        /// <summary>
        /// Hook All Required Events
        /// </summary>
        private void HookAllRequiredEvents()
        {
            try
            {
                _communicationController.OrderExecutionLoginRequest += LoginRequestArrived;
                _communicationController.OrderExecutionLogoutRequest += LogoutRequestArrived;

                _communicationController.MarketOrderRequest += MarketOrderArrived;
                _communicationController.LimitOrderRequest += LimitOrderArrived;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "HookAllRequiredEvents");
            }
        }

        /// <summary>
        /// Methord Fired when Limit Order Arrived.
        /// </summary>
        /// <param name="obj"></param>
        private void LimitOrderArrived(LimitOrder obj)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(obj.ToString(), _type.FullName, "LimitOrderArrived");
                }
                _simulateLimitOrder.NewLimitOrderArrived(obj);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "LimitOrderArrived");
            }
        }

        /// <summary>
        /// Methord Fired when Market Order Arrives.
        /// </summary>
        /// <param name="obj"></param>
        private void MarketOrderArrived(MarketOrder obj)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(obj.ToString(), _type.FullName, "MarketOrderArrived");
                }
                _simulateMarketOrder.NewMarketOrderArrived(obj);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "MarketOrderArrived");
            }
        }

        /// <summary>
        /// Called when Login Request Arrives from OEE.
        /// </summary>
        public void LoginRequestArrived()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("OEE Requesting Login", _type.FullName, "LoginRequestArrived");
                }
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Login Request Accepted Sending it back to OEE", _type.FullName, "LoginRequestArrived");
                }

                _communicationController.PublishOrderAdminMessageResponse("OrderLogin");
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "LoginRequestArrived");
            }
        }

        /// <summary>
        /// Called when Logout Request Arrives from OEE.
        /// </summary>
        public void LogoutRequestArrived()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("OEE Requesting Logout", _type.FullName, "LogoutRequestArrived");
                }

                _simulateMarketOrder.OnSimulatedMarketDisconnect();
                _simulateLimitOrder.OnSimulatedMarketDisconnect();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "LogoutRequestArrived");
            }
        }

        /// <summary>
        /// New Bar Arrived From MarketDataController
        /// </summary>
        /// <param name="bar"></param>
        public void NewBarArrived(Bar bar)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(bar.ToString(), _type.FullName, "NewBarArrived");
                }
                _blockingCollectionBarData.TryAdd(bar);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "NewBarArrived");
            }
        }

        /// <summary>
        /// New Tick Arrived From MarketDataController
        /// </summary>
        /// <param name="tick"></param>
        public void NewTickArrived(Tick tick)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug(tick.ToString(), _type.FullName, "NewTickArrived");
                }

                // Add to ticks Collection
                _simulateMarketOrder.TicksCollection.Add(tick.DateTime, tick);
                lock (_simulateLimitOrder.TicksCollection)
                {
                    _simulateLimitOrder.TicksCollection.Add(tick.DateTime, tick);
                }

                //// Add to blocking collection
                //_blockingCollectionTickData.TryAdd(tick);

                long sequenceNo = _tickRingBuffer.Next();
                Tick entry = _tickRingBuffer[sequenceNo];

                entry.Security = tick.Security;
                entry.MarketDataProvider = tick.MarketDataProvider;
                entry.DateTime = tick.DateTime;

                entry.AskPrice = tick.AskPrice;
                entry.AskSize = tick.AskSize;

                entry.BidPrice = tick.BidPrice;
                entry.BidSize = tick.BidSize;

                entry.LastPrice = tick.LastPrice;
                entry.LastSize = tick.LastSize;

                _tickRingBuffer.Publish(sequenceNo);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "NewTickArrived");
            }
        }

        /// <summary>
        /// Read Bar Data From Queue
        /// </summary>
        public void ReadBarFromQueue()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        //Thread.Sleep(100);
                        Bar bar = _blockingCollectionBarData.Take();
                        if (_tickDataCount <= 0)
                        {
                            if (Logger.IsDebugEnabled)
                            {
                                Logger.Debug("New Bar:" + bar, _type.FullName, "ReadBarFromQueue");
                            }
                            _simulateMarketOrder.NewBarArrived(bar);
                            _simulateLimitOrder.NewBarArrived(bar);
                        }
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception, _type.FullName, "ReadBarFromQueue");
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadTicksFromQueue");
            }
        }

        /// <summary>
        /// Read Ticks From Queue
        /// </summary>
        public void ReadTickFromQueue()
        {
            try
            {
                while (true)
                {
                    //Thread.Sleep(90);
                    Tick tick = _blockingCollectionTickData.Take();
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("New Tick:" + tick, _type.FullName, "ReadTickFromQueue");
                    }
                    //_simulateMarketOrder.NewTickArrived(tick);
                    _simulateLimitOrder.NewTickArrived(tick);

                    //Interlocked.Decrement(ref _tickDataCount);
                }

            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadTicksFromQueue");
            }
        }

        /// <summary>
        /// Sets the tick data count
        /// </summary>
        /// <param name="count">total ticks</param>
        public void SetTickDataCount(int count)
        {
            Interlocked.Add(ref _tickDataCount, count);
        }

        #region Implementation of IEventHandler<in Tick>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(Tick data, long sequence, bool endOfBatch)
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("New Tick:" + data, _type.FullName, "OnNext");
            }
            _simulateLimitOrder.NewTickArrived(data);
        }

        #endregion

        #region Implementation of IEventHandler<in MarketDataObject>

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(MarketDataObject data, long sequence, bool endOfBatch)
        {
            // Use the Object value if it contains valid Tick
            if(data.IsTick)
            {
                _simulateLimitOrder.NewTickArrived(data.Tick);
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New Tick:" + data.Tick.ToString(), _type.FullName, "OnNext");
                }
            }
        }

        #endregion
    }
}
