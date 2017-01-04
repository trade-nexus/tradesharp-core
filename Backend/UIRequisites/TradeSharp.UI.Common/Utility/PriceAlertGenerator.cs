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


using System;
using System.Collections.Generic;
using System.ComponentModel;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.Utility;
using TradeSharp.UI.Common.ValueObjects;

namespace TradeSharp.UI.Common.Utility
{
    /// <summary>
    /// Checks price against specified conditions to generate alert signals
    /// </summary>
    public class PriceAlertGenerator : INotifyPropertyChanged
    {
        private Type _type = typeof (PriceAlertGenerator);

        /// <summary>
        /// Contains alert conditions for BID price
        /// </summary>
        private List<PriceAlertCondition> _bidPriceConditions;

        /// <summary>
        /// Contains alert conditions for ASK price
        /// </summary>
        private List<PriceAlertCondition> _askPriceConditions;

        /// <summary>
        /// Contains alert conditions for TRADE price
        /// </summary>
        private List<PriceAlertCondition> _tradePriceConditions;

        #region Properties

        /// <summary>
        /// Indicates if alert for BID price is required
        /// </summary>
        public IReadOnlyList<PriceAlertCondition> BidPriceConditions
        {
            get { return _bidPriceConditions.AsReadOnly(); }
        }

        /// <summary>
        /// Indicates if alert for ASK price is required
        /// </summary>
        public IReadOnlyList<PriceAlertCondition> AskPriceConditions
        {
            get { return _askPriceConditions.AsReadOnly(); }
        }

        /// <summary>
        /// Indicates if alert for TRADE price is required
        /// </summary>
        public IReadOnlyList<PriceAlertCondition> TradePriceConditions
        {
            get { return _tradePriceConditions.AsReadOnly(); }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public PriceAlertGenerator()
        {
            // Initialize fields
            _bidPriceConditions = new List<PriceAlertCondition>();
            _askPriceConditions = new List<PriceAlertCondition>();
            _tradePriceConditions = new List<PriceAlertCondition>();
        }

        /// <summary>
        /// Adds new alert conditions for BIDS
        /// </summary>
        public void AddBidAlertConditions(List<PriceAlertCondition> alertConditions)
        {
            _bidPriceConditions.Clear();

            // Copy all incoming conditions
            foreach (var condition in alertConditions)
            {
                _bidPriceConditions.Add(condition);   
            }
        }

        /// <summary>
        /// Adds new alert conditions for ASKS
        /// </summary>
        public void AddAskAlertConditions(List<PriceAlertCondition> alertConditions)
        {
            _askPriceConditions.Clear();

            // Copy all incoming conditions
            foreach (var condition in alertConditions)
            {
                _askPriceConditions.Add(condition);
            }
        }

        /// <summary>
        /// Adds new alert conditions for TRADES
        /// </summary>
        public void AddTradeAlertConditions(List<PriceAlertCondition> alertConditions)
        {
            _tradePriceConditions.Clear();

            // Copy all incoming conditions
            foreach (var condition in alertConditions)
            {
                _tradePriceConditions.Add(condition);
            }
        }

        /// <summary>
        /// Evaluates the specified conditions based on incoming prices
        /// </summary>
        /// <param name="tick">Contains complete market details for the given Symbol</param>
        public void EvaluateConditions(Tick tick)
        {
            // Process BID conditions
            if (tick.HasBid)
            {
                EvaluateBidConditions(tick.BidPrice);
            }
            
            // Process ASK conditions
            if (tick.HasAsk)
            {
                EvaluateAskConditions(tick.AskPrice);
            }

            // Process TRADE conditions
            if (tick.HasTrade)
            {
                EvaluateTradeConditions(tick.LastPrice);
            }
        }

        /// <summary>
        /// Evaluates specified BID conditions
        /// </summary>
        /// <param name="price">Current BID price</param>
        private void EvaluateBidConditions(decimal price)
        {
            foreach (var condition in _bidPriceConditions)
            {
                // Evaluate each individual condition for BID
                if (condition.Evaluate(price))
                {
                    // Create new Alert message
                    string title = "BID Alert";
                    string conditionString = EnumUtility.GetEnumDescription(condition.ConditionOperator);
                    string summary = @"Current BID Price: '" + price + "' is " +
                                     "" + conditionString.ToUpper() + " '" + condition.ConditionPrice + "'";

                    AlertMessage alertMessage = new AlertMessage(title, summary);
                    
                    // Notify
                    NotifyEvents(alertMessage);
                }
            }
        }

        /// <summary>
        /// Evaluates specified ASK conditions
        /// </summary>
        /// <param name="price">Current ASK price</param>
        private void EvaluateAskConditions(decimal price)
        {
            foreach (var condition in _askPriceConditions)
            {
                // Evaluate each individual condition for ASK
                if (condition.Evaluate(price))
                {
                    // Create new Alert message
                    string title = "ASK Alert";
                    string conditionString = EnumUtility.GetEnumDescription(condition.ConditionOperator);
                    string summary = @"Current ASK Price: '" + price + "' is " +
                                     "" + conditionString.ToUpper() + " '" + condition.ConditionPrice + "'";

                    AlertMessage alertMessage = new AlertMessage(title, summary);

                    // Notify
                    NotifyEvents(alertMessage);
                }
            }
        }

        /// <summary>
        /// Evaluates specified BID conditions
        /// </summary>
        /// <param name="price">Current TRADE price</param>
        private void EvaluateTradeConditions(decimal price)
        {
            foreach (var condition in _tradePriceConditions)
            {
                // Evaluate each individual condition for TRADE
                if (condition.Evaluate(price))
                {
                    // Create new Alert message
                    string title = "TRADE Alert";
                    string conditionString = EnumUtility.GetEnumDescription(condition.ConditionOperator);
                    string summary = @"Current TRADE Price: '" + price + "' is " +
                                     "" + conditionString.ToUpper() + " '" + condition.ConditionPrice + "'";

                    AlertMessage alertMessage = new AlertMessage(title, summary);

                    // Notify
                    NotifyEvents(alertMessage);
                }
            }
        }

        /// <summary>
        /// Publishes events to notify listeners about new Alerts
        /// </summary>
        /// <param name="message"></param>
        private void NotifyEvents(AlertMessage message)
        {
            // Rasie Events
            EventSystem.Publish<AlertMessage>(message);
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
