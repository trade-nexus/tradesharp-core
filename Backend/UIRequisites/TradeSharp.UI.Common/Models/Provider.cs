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
using System.ComponentModel;
using TradeHub.Common.Core.Constants;
using TradeSharp.UI.Common.Constants;

namespace TradeSharp.UI.Common.Models
{
    /// <summary>
    /// Generic Provider class used for Market Data Provider or Order Execution Provider
    /// </summary>
    public class Provider : INotifyPropertyChanged
    {
        #region Fields

        private ProviderType _providerType;
        private string _providerName;
        private ConnectionStatus _connectionStatus;
        private List<ProviderCredential> _providerCredentials;

        #endregion

        #region Constructors

        public Provider()
        {
            // Initialize Maps
            _providerCredentials = new List<ProviderCredential>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Provider name
        /// </summary>
        public string ProviderName
        {
            get { return _providerName; }
            set
            {
                if (_providerName != value)
                {
                    _providerName = value;
                    OnPropertyChanged("ProviderName");
                }
            }
        }

        /// <summary>
        /// Provider connection status
        /// </summary>
        public ConnectionStatus ConnectionStatus
        {
            get { return _connectionStatus; }
            set
            {
                if (_connectionStatus != value)
                {
                    _connectionStatus = value;
                    OnPropertyChanged("ConnectionStatus");
                }
            }
        }

        /// <summary>
        /// List of credentials for provider (i.e. Username, Password, IpAddress etc.)
        /// </summary>
        public List<ProviderCredential> ProviderCredentials
        {
            get { return _providerCredentials; }
            set
            {
                if (_providerCredentials != value)
                {
                    _providerCredentials = value;
                    OnPropertyChanged("Credentials");
                }
            }
        }

        /// <summary>
        /// Type of Provider e.g Market Data, Order Execution, etc.
        /// </summary>
        public ProviderType ProviderType
        {
            get { return _providerType; }
            set { _providerType = value; }
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
