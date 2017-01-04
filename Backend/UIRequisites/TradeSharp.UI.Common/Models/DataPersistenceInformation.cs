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


using System.ComponentModel;

namespace TradeSharp.UI.Common.Models
{
    /// <summary>
    /// Contains information regarding data to be persisted
    /// </summary>
    public class DataPersistenceInformation : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Indicates if given market data Trades need to be persisted
        /// </summary>
        private bool _saveTrades = false;

        /// <summary>
        /// Indicates if given market data Quotes need to be persisted
        /// </summary>
        private bool _saveQuotes = false;

        /// <summary>
        /// Indicates if given market data Bars need to be persisted
        /// </summary>
        private bool _saveBars = false;

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DataPersistenceInformation()
        {
            
        }

        #region Properties

        /// <summary>
        /// Indicates if given market data Trades need to be persisted
        /// </summary>
        public bool SaveTrades
        {
            get { return _saveTrades; }
            set
            {
                _saveTrades = value;
                OnPropertyChanged("SaveTrades");
            }
        }

        /// <summary>
        /// Indicates if given market data Quotes need to be persisted
        /// </summary>
        public bool SaveQuotes
        {
            get { return _saveQuotes; }
            set
            {
                _saveQuotes = value;
                OnPropertyChanged("SaveQuotes");
            }
        }

        /// <summary>
        /// Indicates if given market data Bars need to be persisted
        /// </summary>
        public bool SaveBars
        {
            get { return _saveBars; }
            set
            {
                _saveBars = value;
                OnPropertyChanged("SaveBars");
            }
        }

        #endregion

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
