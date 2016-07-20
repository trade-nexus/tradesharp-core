using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories.Parameters;
using TradeHub.Common.Core.ValueObjects;
using TradeHub.ReportingEngine.Client.Service;

namespace TradeHub.StrategyEngine.ReportService
{
    /// <summary>
    /// Provides access to Reporting Engine Server using Client underneath
    /// </summary>
    public class ReportingEngineService : IEventHandler<Dictionary<OrderParameters, string>>, IEventHandler<Dictionary<TradeParameters, string>>
    {
        private Type _type = typeof (ReportingEngineService);

        /// <summary>
        /// Provides access to Reporting Engine - Server
        /// </summary>
        private readonly ReportingEngineClient _reportingEngineClient;

        /// <summary>
        /// Raised when Order Report is received from the Server
        /// </summary>
        public event Action<IList<object[]>> OrderReportReceivedEvent;

        /// <summary>
        /// Raised when Profit Loss Report is received
        /// </summary>
        public event Action<ProfitLossStats> ProfitLossReportReceivedEvent;

        #region Disruptor

        /// <summary>
        /// Disruptor Ring Buffer Size 
        /// </summary>
        private readonly int _ringSize = 1024;  // Must be multiple of 2

        /// <summary>
        ///  Order Report Request disruptor object
        /// </summary>
        private Disruptor<Dictionary<OrderParameters, string>> _orderReportRequestDisruptor;

        /// <summary>
        ///  Profit Loss Report Request disruptor object
        /// </summary>
        private Disruptor<Dictionary<TradeParameters, string>> _pnlReportRequestDisruptor;

        /// <summary>
        /// Ring buffer to be used with Order Report Request disruptor
        /// </summary>
        private RingBuffer<Dictionary<OrderParameters, string>> _orderReportRequestRingBuffer;

        /// <summary>
        /// Ring buffer to be used with Profit Loss Report Request disruptor
        /// </summary>
        private RingBuffer<Dictionary<TradeParameters, string>> _pnlReportRequestRingBuffer;

        /// <summary>
        /// Publishes messages to Order Report Request Disruptor
        /// </summary>
        private EventPublisher<Dictionary<OrderParameters, string>> _orderReportRequestMessagePublisher;

        /// <summary>
        /// Publishes messages to Profit Loss Report Request Disruptor
        /// </summary>
        private EventPublisher<Dictionary<TradeParameters, string>> _pnlReportRequestMessagePublisher;

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="reportingEngineClient">Provides access to Reporting Engine - Server</param>
        public ReportingEngineService(ReportingEngineClient reportingEngineClient)
        {
            // Save Instance
            _reportingEngineClient = reportingEngineClient;

            // Register Events
            SubscribeClientEvents();

            // Initialize Disruptor and relevent resources
            InitializeDisruptor();
        }

        /// <summary>
        /// Subscribes Reporting Engine Client Events to receive data
        /// </summary>
        private void SubscribeClientEvents()
        {
            // Makes sure that events are not hooked multiple times
            UnsubscribeClientEvents();

            // Register Events
            _reportingEngineClient.OrderReportReceivedEvent += OrderReportReceived;
            _reportingEngineClient.ProfitLossReportReceivedEvent += ProfitLossReportReceived;
        }

        /// <summary>
        /// Un-subscribes Reporting Engine Client Events
        /// </summary>
        private void UnsubscribeClientEvents()
        {
            // Un-Subscribe Events
            _reportingEngineClient.OrderReportReceivedEvent -= OrderReportReceived;
            _reportingEngineClient.ProfitLossReportReceivedEvent -= ProfitLossReportReceived;
        }

        /// <summary>
        /// Initilaizes Disruptor and relavent resources
        /// </summary>
        private void InitializeDisruptor()
        {
            // Initialize Disruptor
            _orderReportRequestDisruptor = new Disruptor<Dictionary<OrderParameters, string>>(() => new Dictionary<OrderParameters, string>(), _ringSize, TaskScheduler.Default);
            _pnlReportRequestDisruptor = new Disruptor<Dictionary<TradeParameters, string>>(() => new Dictionary<TradeParameters, string>(), _ringSize, TaskScheduler.Default);

            // Set Event Handler
            _orderReportRequestDisruptor.HandleEventsWith(this);
            _pnlReportRequestDisruptor.HandleEventsWith(this);

            // Start Ring Buffer
            _orderReportRequestRingBuffer = _orderReportRequestDisruptor.Start();
            _pnlReportRequestRingBuffer = _pnlReportRequestDisruptor.Start();

            // Start Event Publisher
            _orderReportRequestMessagePublisher = new EventPublisher<Dictionary<OrderParameters, string>>(_orderReportRequestRingBuffer);
            _pnlReportRequestMessagePublisher = new EventPublisher<Dictionary<TradeParameters, string>>(_pnlReportRequestRingBuffer);
        }


        #region Start/Stop

        /// <summary>
        /// Starts Reporting Engine Service
        /// </summary>
        /// <returns>Indicates whether the operation was successful or not.</returns>
        public bool StartService()
        {
            if (_reportingEngineClient != null)
            {
                // Start Client
                _reportingEngineClient.Connect();

                return true;
            }

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Client object not initialized.", _type.FullName, "StartService");
            }

            return false;
        }

        /// <summary>
        /// Stops Reporting Engine Service
        /// </summary>
        /// <returns>Indicates whether the operation was successful or not.</returns>
        public bool StopService()
        {
            if (_reportingEngineClient != null)
            {
                // Stop Client
                _reportingEngineClient.Disconnect();

                return true;
            }

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Client object not initialized.", _type.FullName, "StopService");
            }

            return false;
        }

        #endregion

        #region Request Messages

        /// <summary>
        /// Sends Order Report request to Server via Client
        /// </summary>
        /// <param name="parameters">Search parameters for the report</param>
        public void RequestOrderReport(Dictionary<OrderParameters, string> parameters)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New Order Report request received", _type.FullName, "RequestOrderReport");
                }

                // Move request to disruptor
                _orderReportRequestMessagePublisher.PublishEvent((requestParameters, sequenceNo) =>
                {
                    // Clear existing data
                    requestParameters.Clear();

                    // Copy information
                    foreach (KeyValuePair<OrderParameters, string> keyValuePair in parameters)
                    {
                        requestParameters.Add(keyValuePair.Key, keyValuePair.Value);
                    }

                    // Return updated object
                    return requestParameters;
                });
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RequestOrderReport");
            }
        }

        /// <summary>
        /// Sends Profit Loss Report request to Server via Client
        /// </summary>
        /// <param name="parameters">Search parameters for the report</param>
        public void RequestProfitLossReport(Dictionary<TradeParameters, string> parameters)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("New Profit Report request received", _type.FullName, "RequestProfitLossReport");
                }

                // Move request to disruptor
                _pnlReportRequestMessagePublisher.PublishEvent((requestParameters, sequenceNo) =>
                {
                    // Clear existing data
                    requestParameters.Clear();

                    // Copy information
                    foreach (KeyValuePair<TradeParameters, string> keyValuePair in parameters)
                    {
                        requestParameters.Add(keyValuePair.Key, keyValuePair.Value);
                    }

                    // Return updated object
                    return requestParameters;
                });
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RequestProfitLossReport");
            }
        }

        #endregion

        #region Response Messages

        /// <summary>
        /// Called when requested Order Report is received
        /// </summary>
        /// <param name="report">Contains information for the requested report</param>
        private void OrderReportReceived(IList<object[]> report)
        {
            // Raise Event to notify listeners
            if (OrderReportReceivedEvent != null)
            {
                OrderReportReceivedEvent(report);
            }
        }

        /// <summary>
        /// Called when requested Profit Loss Report is received
        /// </summary>
        /// <param name="report">Contains information for the requested report</param>
        private void ProfitLossReportReceived(ProfitLossStats report)
        {
            // Raise Event to notify listeners
            if (ProfitLossReportReceivedEvent != null)
            {
                ProfitLossReportReceivedEvent(report);
            }
        }

        #endregion

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(Dictionary<OrderParameters, string> data, long sequence, bool endOfBatch)
        {
            // Send Request to Client
            _reportingEngineClient.RequestOrderReport(data);
        }

        /// <summary>
        /// Called when a publisher has committed an event to the <see cref="T:Disruptor.RingBuffer`1"/>
        /// </summary>
        /// <param name="data">Data committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="sequence">Sequence number committed to the <see cref="T:Disruptor.RingBuffer`1"/></param><param name="endOfBatch">flag to indicate if this is the last event in a batch from the <see cref="T:Disruptor.RingBuffer`1"/></param>
        public void OnNext(Dictionary<TradeParameters, string> data, long sequence, bool endOfBatch)
        {
            // Send Request to Client
            _reportingEngineClient.RequestProfitLossReport(data);
        }
    }
}
