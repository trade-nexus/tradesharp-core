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
using TradeSharp.UI.Common.Constants;

namespace TradeSharp.UI.Common.Models
{
    /// <summary>
    /// Contains necessary information for the application services
    /// </summary>
    public class ServiceDetails : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Indicates if the service is avaiable (Enabled) or not (Disabled)
        /// </summary>
        private bool _enabled;

        /// <summary>
        /// Application Service name
        /// </summary>
        private string _serviceName;

        /// <summary>
        /// Application Service name to be displayed on UI
        /// </summary>
        private string _serviceDisplayName;

        /// <summary>
        /// Current Status of the Service
        /// </summary>
        private ServiceStatus _status;

        #endregion

        #region Properties

        /// <summary>
        /// Indicates if the service is avaiable (Enabled) or not (Disabled)
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value; 
                OnPropertyChanged("Enabled");
            }
        }

        /// <summary>
        /// Application Service name
        /// </summary>
        public string ServiceName
        {
            get { return _serviceName; }
            set
            {
                _serviceName = value; 
                OnPropertyChanged("ServiceName");
            }
        }

        /// <summary>
        /// Current Status of the Service
        /// </summary>
        public ServiceStatus Status
        {
            get { return _status; }
            set
            {
                _status = value; 
                OnPropertyChanged("Status");
            }
        }

        /// <summary>
        /// Application Service name to be displayed on UI
        /// </summary>
        public string ServiceDisplayName
        {
            get { return _serviceDisplayName; }
            set
            {
                _serviceDisplayName = value;
                OnPropertyChanged("ServiceDisplayName");
            }
        }

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="serviceName">Application Service name</param>
        /// <param name="serviceStatus">Current Status of the Service</param>
        public ServiceDetails(string serviceName, ServiceStatus serviceStatus)
        {
            _serviceName = serviceName;
            _status = serviceStatus;
            _serviceDisplayName = serviceName;
        }

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
