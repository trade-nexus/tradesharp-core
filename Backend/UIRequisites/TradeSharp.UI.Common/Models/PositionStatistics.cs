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
using TradeHub.Common.Core.DomainModels;

namespace TradeSharp.UI.Common.Models
{
    /// <summary>
    /// Basic Position information for a particular Security (Symbol)
    /// </summary>
    public class PositionStatistics : INotifyPropertyChanged
    {
        /// <summary>
        /// Contains Symbol information
        /// </summary>
        private Security _security;

        /// <summary>
        /// Position on given Security
        /// </summary>
        private int _position;

        /// <summary>
        /// PnL on given Security
        /// </summary>
        private decimal _pnl;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="security">Contains Symbol Information</param>
        public PositionStatistics(Security security)
        {
            _security = security;
            _position = default(int);
            _pnl = default(decimal);
        }

        #region Properties

        /// <summary>
        /// Contains Symbol information
        /// </summary>
        public Security Security
        {
            get { return _security; }
            set { _security = value; }
        }

        /// <summary>
        /// Position on given Security
        /// </summary>
        public int Position
        {
            get { return _position; }
            set
            {
                _position = value;
                OnPropertyChanged("Position");
            }
        }

        /// <summary>
        /// PnL on given Security
        /// </summary>
        public decimal Pnl
        {
            get { return _pnl; }
            set
            {
                _pnl = value;
                OnPropertyChanged("Pnl");
            }
        }

        #endregion

        #region INotifyPropertyChanged members

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
