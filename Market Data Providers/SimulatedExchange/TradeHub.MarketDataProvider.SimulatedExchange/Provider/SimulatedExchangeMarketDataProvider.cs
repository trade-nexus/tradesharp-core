
using System;
using EasyNetQ;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.MarketDataProvider;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataProvider.SimulatedExchange.Service;
using TradeHub.SimulatedExchange.Common;
using TradeHub.SimulatedExchange.DomainObjects;
using TradeHub.SimulatedExchange.DomainObjects.Constant;

namespace TradeHub.MarketDataProvider.SimulatedExchange.Provider
{
    public class SimulatedExchangeMarketDataProvider : ILiveBarDataProvider, ILiveTickDataProvider, IHistoricBarDataProvider
    {
        private Type _type = typeof(SimulatedExchangeMarketDataProvider);

        private CommunicationController _communicationController;
        public IMarketDataControler MarketDataControler;

        private bool _isConnected = false;

        public SimulatedExchangeMarketDataProvider()
        {
            try
            {
                _communicationController = new CommunicationController();
                SubscribeRequiredEvents();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SimulatedExchangeMarketDataProvider");
            }
        }

        /// <summary>
        /// Connects/Starts a client
        /// </summary>
        public bool Start()
        {
            if (!_isConnected)
                _communicationController.Connect();

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Starting Simulated Data Exchange Connector", _type.FullName, "Start");
            }

            //_communicationController.PublishMarketAdminMessage("DataLogin");

            // Raise Login Event 
            LoginArrived();

            return true;
        }

        /// <summary>
        /// Subscribe All Events.
        /// </summary>
        private void SubscribeRequiredEvents()
        {
            _communicationController.MarketDataLoginRequest += LoginArrived;

            _communicationController.BarData += MarketDataControlerBarArrived;
            _communicationController.TickData += MarketDataControlerTickArrived;
            _communicationController.HistoricData += MarketDataControlerHistoricBarArrived;
        }

        /// <summary>
        /// Disconnect/Stops a client
        /// </summary>
        public bool Stop()
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Stoping Simulated Exchange Connector", _type.FullName, "Stop");
                }

                _isConnected = false;
                _communicationController.Disconnect();

                if (LogoutArrived!=null)
                {
                    LogoutArrived.Invoke(Common.Core.Constants.MarketDataProvider.SimulatedExchange);
                }
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Stop");
                return false;
            }
        }

        public bool IsConnected()
        {
            return _isConnected;
        }

        public event Action<string> LogonArrived;
        public event Action<string> LogoutArrived;
        public event Action<MarketDataEvent> MarketDataRejectionArrived;
        public event Action<Bar, string> BarArrived;
        public event Action<Tick> TickArrived;
        public event Action<HistoricBarData> HistoricBarDataArrived;

        /// <summary>
        /// Market data request message
        /// </summary>
        public bool SubscribeTickData(Subscribe request)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Sending Tick Subscription to Simulated Exchange " + request, _type.FullName, "SubscribeTickData");
            }
            try
            {
                _communicationController.PublishTickRequest(request);
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeTickData");
            }
            return false;
        }

        /// <summary>
        /// Unsubscribe Market data message
        /// </summary>
        public bool UnsubscribeTickData(Unsubscribe request)
        {
            return true;
        }

        /// <summary>
        /// Subscribe Live Bars
        /// </summary>
        /// <param name="barDataRequest"></param>
        public bool SubscribeBars(BarDataRequest barDataRequest)
        {
            //MarketDataControler.SubscribeSymbol(barDataRequest);
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Sending Subscription to Simulated Exchange " + barDataRequest, _type.FullName, "SubscribeBars");
            }
            try
            {
                _communicationController.PublishBarDataRequest(barDataRequest);
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SubscribeBars");
            }
            return false;
        }

        /// <summary>
        /// Unsubscribe Live Bars
        /// </summary>
        /// <param name="barDataRequest"></param>
        public bool UnsubscribeBars(BarDataRequest barDataRequest)
        {
            return true;
        }

        /// <summary>
        /// Historic Bar Data Request Message
        /// </summary>
        public bool HistoricBarDataRequest(HistoricDataRequest historicDataRequest)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Sending historic data request to Simulated Exchange " + historicDataRequest, _type.FullName, "HistoricBarDataRequest");
                }

                _communicationController.PublishHistoricDataRequest(historicDataRequest);
                return true;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "HistoricBarDataRequest");
                return false;
            }
        }

        /// <summary>
        /// Methord Fired on login arrived from Simulated Exchange.
        /// </summary>
        private void LoginArrived()
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Simulated Exchange Logon Arrived", _type.FullName, "LoginArrived");
            }
            
            _isConnected = true;
            
            if (LogonArrived != null)
            {
                LogonArrived.Invoke(Common.Core.Constants.MarketDataProvider.SimulatedExchange);
            }
        }

        /// <summary>
        /// Raised when a new Bar Arrives from the MarketDataControler
        /// </summary>
        private void MarketDataControlerBarArrived(Bar bar)
        {
            if (BarArrived!=null)
            {
                BarArrived.Invoke(bar,bar.RequestId);
            }
        }

        /// <summary>
        /// Raised when new Tick is recieved from Simulated Exchange
        /// </summary>
        /// <param name="tick">Contains required Tick Info</param>
        private void MarketDataControlerTickArrived(Tick tick)
        {
            if (TickArrived != null)
            {
                TickArrived.Invoke(tick);
            }
        }

        /// <summary>
        /// Raised when Historical bar data is received from Simulated Exchange
        /// </summary>
        /// <param name="historicBarData">Contains required Historical Data Info</param>
        private void MarketDataControlerHistoricBarArrived(HistoricBarData historicBarData)
        {
            if (HistoricBarDataArrived != null)
            {
                HistoricBarDataArrived(historicBarData);
            }
        }

    }
}
