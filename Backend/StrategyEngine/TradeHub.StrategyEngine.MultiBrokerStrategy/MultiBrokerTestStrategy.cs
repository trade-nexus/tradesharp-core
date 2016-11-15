/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.CustomAttributes;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.FactoryMethods;
using TradeHub.Common.Core.Utility;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.StrategyEngine.TradeHub;

namespace TradeHub.StrategyEngine.MultiBrokerStrategy
{
    [TradeHubAttributes("MultiBrokerStrategy", typeof(MultiBrokerTestStrategy))]
    public class MultiBrokerTestStrategy : TradeHubStrategy
    {
        private readonly string _symbolOne;
        private readonly string _symbolTwo;

        private string _dataProviderOne;
        private string _dataProviderTwo;

        private string _executionProviderOne;
        private string _executionProviderTwo;
        
        private MarketDataIdGenerator _idGenerator;
        private List<Subscribe> _subscriptionList;

        public event Action<string> MarketDataLogonArrived;
        public event Action<string> MarketDataLogoutArrived;

        public event Action<string> HistoricalDataLogonArrived;
        public event Action<string> HistoricalDataLogoutArrived;
        
        public event Action<string> OrderExecutionLogonArrived;
        public event Action<string> OrderExecutionLogoutArrived;

        public event Action<Tick> TickArrived;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="symbolOne"></param>
        /// <param name="symbolTwo"></param>
        /// <param name="marketDataProviderOne"></param>
        /// <param name="marketDataProviderTwo"></param>
        /// <param name="orderExecutionProviderOne"></param>
        /// <param name="orderExecutionProviderTwo"></param>
        public MultiBrokerTestStrategy(string symbolOne, string symbolTwo, 
            string marketDataProviderOne, string marketDataProviderTwo, 
            string orderExecutionProviderOne, string orderExecutionProviderTwo)
            : base(
                new[] { marketDataProviderOne, marketDataProviderTwo }, new[] { orderExecutionProviderOne, orderExecutionProviderTwo },
                null)
        {
            _symbolOne = symbolOne;
            _symbolTwo = symbolTwo;
            _dataProviderOne = marketDataProviderOne;
            _dataProviderTwo = marketDataProviderTwo;
            _executionProviderOne = orderExecutionProviderOne;
            _executionProviderTwo = orderExecutionProviderTwo;

            _idGenerator = new MarketDataIdGenerator();
            _subscriptionList = new List<Subscribe>();
        }

        /// <summary>
        /// Overriden to provides additional funtionality for base class function 'Run()'
        /// </summary>
        protected override void OnRun()
        {
        }

        /// <summary>
        /// Overriden to provides additional funtionality for base class function 'Stop()'
        /// </summary>
        protected override void OnStop()
        {
            foreach (var subscribe in _subscriptionList)
            {
                Unsubscribe unsubscribe = SubscriptionMessage.TickUnsubscription(_idGenerator.NextTickId(), subscribe.Security, subscribe.MarketDataProvider);
                Unsubscribe(unsubscribe);
            }
        }

        #region Connectivity Notifications

        /// <summary>
        /// Called when Market Data Service is connected to MDE-Server
        /// </summary>
        public override void OnMarketDataServiceConnected()
        {
            foreach (var marketDataProviderName in MarketDataProviderNames)
            {
                base.LogonRequestMarketData(marketDataProviderName);
            }
        }

        /// <summary>
        /// Called when Order Execution Service is connected to OEE-Server
        /// </summary>
        public override void OnOrderExecutionServiceConnected()
        {
            foreach (string orderExecutionProviderName in OrderExecutionProviderNames)
            {
                base.LogonRequestOrderExecution(orderExecutionProviderName);
            }
        }

        /// <summary>
        /// Called when Historical Data Service is connected to MDE-Server
        /// </summary>
        public override void OnHistoricalDataServiceConnected()
        {
            foreach (var historicalDataProviderName in HistoricalDataProviderNames)
            {
                base.LogonRequestHistoricalData(historicalDataProviderName);
            }
        }

        /// <summary>
        /// Called when Logon is received from Market Data Service
        /// </summary>
        /// <param name="marketDataProvider">Name of the market data provider</param>
        public override void OnMarketDataServiceLogonArrived(string marketDataProvider)
        {
            if (MarketDataLogonArrived != null)
            {
                MarketDataLogonArrived(marketDataProvider);
            }

            base.DisplayMessage("Market data LOGON arrived for: " + marketDataProvider);

            // Request Market Data
            string id = _idGenerator.NextTickId();
            Security security = new Security()
            {
                Symbol = marketDataProvider.Equals(_dataProviderOne) ? _symbolOne : _symbolTwo
            };

            RequestTickData(id, security, marketDataProvider);
        }

        /// <summary>
        /// Called when logon is received from Order Execution Service
        /// </summary>
        /// <param name="orderExecutionProvider">Name of the order execution provider</param>
        public override void OnOrderExecutionServiceLogonArrived(string orderExecutionProvider)
        {
            if (OrderExecutionLogonArrived != null)
            {
                OrderExecutionLogonArrived(orderExecutionProvider);
            }

            base.DisplayMessage("Order execution LOGON arrived for: " + orderExecutionProvider);

            // Send Market Order
            string id = GetNewOrderId();

            Security security = new Security()
            {
                Symbol = orderExecutionProvider.Equals(_executionProviderOne) ? _symbolOne : _symbolTwo
            };

            SendMarketOrder(id, security, 1000, orderExecutionProvider);
        }

        /// <summary>
        /// Called when Logon is received from Historical Data Service
        /// </summary>
        /// <param name="historicalDataProvider">Name of the historical data provider</param>
        public override void OnHistoricalDataServiceLogonArrived(string historicalDataProvider)
        {
        }

        /// <summary>
        /// Called when logout is received from Market Data Service
        /// </summary>
        /// <param name="marketDataProvider">Name of the market data provider</param>
        public override void OnMarketDataServiceLogoutArrived(string marketDataProvider)
        {
        }

        /// <summary>
        /// Called when logout is received from Order Execution Service
        /// </summary>
        /// <param name="orderExecutionProvider">Name of the order execution provider</param>
        public override void OnOrderExecutionServiceLogoutArrived(string orderExecutionProvider)
        {
        }

        /// <summary>
        /// Called when Logout is received from Historical Data Service
        /// </summary>
        /// <param name="marketDataProvider">Name of the historical data provider</param>
        public override void OnHistoricalDataServiceLogoutArrived(string marketDataProvider)
        {
        }

        #endregion

        #region Request Data

        /// <summary>
        /// Sends request for live tick data
        /// </summary>
        /// <param name="id">Unique ID to distinguish the request</param>
        /// <param name="security">Contains symbol information</param>
        /// <param name="marketDataProider">Market Data Provider from which to receive data</param>
        private void RequestTickData(string id, Security security, string marketDataProider)
        {
            Subscribe subscribe = SubscriptionMessage.TickSubscription(id, security, marketDataProider);

            _subscriptionList.Add(subscribe);

            base.Subscribe(subscribe);
        }

        /// <summary>
        /// Sends request to receive live bars
        /// </summary>
        /// <param name="id">Unique ID to distinguish the request</param>
        /// <param name="security">Contains symbol information</param>
        /// <param name="length">Bar length in seconds</param>
        /// <param name="pipSize">Smallest pip size for the bars</param>
        /// <param name="barSeed">Seed value to be used for bar generation</param>
        private void RequestLiveBars(string id, Security security, decimal length, decimal pipSize, decimal barSeed)
        {
        }

        /// <summary>
        /// Sends request to retrieve historical bar data
        /// </summary>
        /// <param name="id">Unique ID to distinguish the request</param>
        /// <param name="security">Contains symbol information</param>
        /// <param name="startTime">Time from which to start the historical bar data</param>
        /// <param name="endTime">Time at which the historical bar data should end</param>
        /// <param name="interval">Bar interval</param>
        private void RequestHistoricalBarData(string id, Security security, DateTime startTime, DateTime endTime, uint interval)
        {
        }

        #endregion

        #region Receivce Data

        /// <summary>
        /// Called when new Tick data is received
        /// </summary>
        /// <param name="tick">Contains live tick/quote information</param>
        public override void OnTickArrived(Tick tick)
        {
            if (TickArrived != null)
            {
                TickArrived(tick);
            }

            base.DisplayMessage(tick.ToString());
        }

        /// <summary>
        /// Called when new Live Bar is received
        /// </summary>
        /// <param name="bar">Contains complete bar detail</param>
        public override void OnBarArrived(Bar bar)
        {
        }

        /// <summary>
        /// Called when Historical Bars are received
        /// </summary>
        /// <param name="historicBarData">Contains all the historical data previously requested</param>
        public override void OnHistoricalDataArrived(HistoricBarData historicBarData)
        {
        }

        #endregion

        #region Send Order

        /// <summary>
        /// Sends a new market order to the server
        /// </summary>
        /// <param name="id">Unique ID to distinguish the order</param>
        /// <param name="security">Contains symbol information</param>
        /// <param name="orderSize">Trade size or lot</param>
        /// <param name="orderExecutionProvider"></param>
        /// <param name="orderSide">Side on which to send the order</param>
        private void SendMarketOrder(string id, Security security, int orderSize, string orderExecutionProvider, string orderSide = OrderSide.BUY)
        {
            // Create new Market Order
            MarketOrder marketOrder = OrderMessage.GenerateMarketOrder(id, security, orderSide, orderSize, orderExecutionProvider);

            // Send market order
            this.SendOrder(marketOrder);
        }

        /// <summary>
        /// Sends a new limit order to the server
        /// </summary>
        /// <param name="id">Unique ID to distinguish the order</param>
        /// <param name="security">Contains symbol information</param>
        /// <param name="orderSize">Trade size or lot</param>
        /// <param name="limitPrice">Limit price</param>
        /// <param name="orderExecutionProvider"></param>
        /// <param name="orderSide">Side on which to send the order</param>
        private void SendLimitOrder(string id, Security security, int orderSize, decimal limitPrice, string orderExecutionProvider, string orderSide = OrderSide.BUY)
        {
            // Create new Limit Order
            LimitOrder limitOrder = OrderMessage.GenerateLimitOrder(id, security, orderSide, orderSize, limitPrice, orderExecutionProvider);

            // Send limit order
            this.SendOrder(limitOrder);
        }

        /// <summary>
        /// Sends request to cancel an active order
        /// </summary>
        /// <param name="orderId">Unique ID of the order to be cancelled</param>
        private void CancelLimitOrder(string orderId)
        {
            // Send order cancellation request
            this.CancelOrder(orderId);
        }

        #endregion

        #region Order Notifications

        /// <summary>
        /// Called when order is accepted by the exchange
        /// </summary>
        /// <param name="order">Contains order details</param>
        public override void OnNewArrived(Order order)
        {
        }

        /// <summary>
        /// Called when order execution is received 
        /// </summary>
        /// <param name="execution">Contains complete execution details</param>
        public override void OnExecutionArrived(Execution execution)
        {
        }

        /// <summary>
        /// Called when order cancellation is received
        /// </summary>
        /// <param name="order">Contains cancelled order information</param>
        public override void OnCancellationArrived(Order order)
        {
        }

        /// <summary>
        /// Called when order is rejected by the exchange
        /// </summary>
        /// <param name="rejection">Contains rejection details</param>
        public override void OnRejectionArrived(Rejection rejection)
        {
        }

        #endregion
    }
}
