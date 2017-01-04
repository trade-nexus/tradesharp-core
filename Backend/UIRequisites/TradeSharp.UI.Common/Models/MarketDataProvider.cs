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


using System.Collections.Generic;
using System.Collections.ObjectModel;
using TradeHub.Common.Core.DomainModels;

namespace TradeSharp.UI.Common.Models
{
    /// <summary>
    /// Contains information specific to Market Data Provider
    /// </summary>
    public class MarketDataProvider : Provider
    {
        #region Fields

        /// <summary>
        /// Contains subscribed symbol's tick information (Valid if the provider is type 'Market Data')
        /// KEY = Symbol
        /// VALUE = <see cref="MarketDataDetail"/>
        /// </summary>
        private Dictionary<string, MarketDataDetail> _marketDetailsMap;

        /// <summary>
        /// Contains all Market Detail objects
        /// </summary>
        private ObservableCollection<MarketDataDetail> _marketDetailCollection;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public MarketDataProvider()
        {
            // Initialize Map
            _marketDetailsMap = new Dictionary<string, MarketDataDetail>();
            _marketDetailCollection = new ObservableCollection<MarketDataDetail>();
        }

        #region Properties

        /// <summary>
        /// Contains all Market Detail objects
        /// </summary>
        public ObservableCollection<MarketDataDetail> MarketDetailCollection
        {
            get { return _marketDetailCollection; }
            set
            {
                _marketDetailCollection = value;
                OnPropertyChanged("MarketDetailCollection");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds Market Detail object to Map/Collection
        /// </summary>
        /// <param name="marketDataDetail">Holds market data information</param>
        public void AddMarketDetail(MarketDataDetail marketDataDetail)
        {
            // Check if the object already exists for the given symbol
            if (!_marketDetailsMap.ContainsKey(marketDataDetail.Security.Symbol))
            {
                // Add object to MAP
                _marketDetailsMap.Add(marketDataDetail.Security.Symbol, marketDataDetail);

                // Add object to collection
                MarketDetailCollection.Add(marketDataDetail);
            }
        }

        /// <summary>
        /// Updates tick information for the given Symbol
        /// </summary>
        /// <param name="symbol">Symbol Name</param>
        /// <param name="tick">Contains market data information</param>
        public void UpdateMarketDetail(string symbol, Tick tick)
        {
            MarketDataDetail marketDetails;

            // Get MarketDataDetail object to update tick information
            if (_marketDetailsMap.TryGetValue(tick.Security.Symbol, out marketDetails))
            {
                // Update collections for Depth information
                marketDetails.Update(tick);
            }
        }

        /// <summary>
        /// Removes tick information for the given Symbol from local maps
        /// </summary>
        /// <param name="symbol">Symbol Name</param>
        public void RemoveMarketInformation(string symbol)
        {
            MarketDataDetail marketDataDetail;

            // Get MarketDataDetail object which is to be removed
            if (_marketDetailsMap.TryGetValue(symbol, out marketDataDetail))
            {
                // Clear depth information
                marketDataDetail.AskRecordsCollection.Clear();
                marketDataDetail.BidRecordsCollection.Clear();

                // remove from local map
                _marketDetailsMap.Remove(symbol);

                // Remove from collection
                MarketDetailCollection.Remove(marketDataDetail);
            }
        }

        /// <summary>
        /// Checks if the given symbol is already loading into application from UI
        /// </summary>
        /// <param name="symbol">Symbol name</param>
        public bool IsSymbolLoaded(string symbol)
        {
            return _marketDetailsMap.ContainsKey(symbol);
        }

        /// <summary>
        /// Checks if Quotes are to be persisted for given symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool IsQuotePersistenceRequired(string symbol)
        {
            MarketDataDetail marketDetail;

            // Get MarketDataDetail object
            if (_marketDetailsMap.TryGetValue(symbol, out marketDetail))
            {
                return marketDetail.PersistenceInformation.SaveQuotes;
            }

            return false;
        }

        /// <summary>
        /// Checks if Trades are to be persisted for given symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool IsTradePersistenceRequired(string symbol)
        {
            MarketDataDetail marketDetail;

            // Get MarketDataDetail object
            if (_marketDetailsMap.TryGetValue(symbol, out marketDetail))
            {
                return marketDetail.PersistenceInformation.SaveTrades;
            }

            return false;
        }

        /// <summary>
        /// Checks if Bars are to be persisted for given symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public bool IsBarPersistenceRequired(string symbol)
        {
            MarketDataDetail marketDetail;

            // Get MarketDataDetail object
            if (_marketDetailsMap.TryGetValue(symbol, out marketDetail))
            {
                return marketDetail.PersistenceInformation.SaveBars;
            }

            return false;
        }

        #endregion
    }
}
