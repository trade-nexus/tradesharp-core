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
    public class ProviderCredential : INotifyPropertyChanged
    {
        #region Fields

        private string _credentialName;
        private string _credentialValue;

        #endregion

        #region Constructors

        public ProviderCredential()
        {

        }
        
        #endregion

        #region Properties

        /// <summary>
        /// Holds credential name, for example 'Username', 'Password' etc.
        /// </summary>
        public string CredentialName
        {
            get { return _credentialName; }
            set
            {
                if (_credentialName != value)
                {
                    _credentialName = value;
                    OnPropertyChanged("CredentialName");
                }
            }
        }

        /// <summary>
        /// Holds credential value
        /// </summary>
        public string CredentialValue
        {
            get { return _credentialValue; }
            set
            {
                if (_credentialValue != value)
                {
                    _credentialValue = value;
                    OnPropertyChanged("CredentialValue");
                }
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
