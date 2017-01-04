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
using System.ComponentModel;

namespace TradeSharp.UI.Common.Models
{
    public class StrategyStatistics : INotifyPropertyChanged
    {
        /// <summary>
        /// Indentifies strategy instance
        /// </summary>
        private string _instanceId;

        /// <summary>
        /// Time of generation
        /// </summary>
        private DateTime _time;

        /// <summary>
        /// Inforamtion to be displayed
        /// </summary>
        private string _information;

        public StrategyStatistics(string instanceId, DateTime time, string information)
        {
            _instanceId = instanceId;
            _time = time;
            _information = information;
        }

        #region Properties

        /// <summary>
        /// Indentifies strategy instance
        /// </summary>
        public string InstanceId
        {
            get { return _instanceId; }
            set { _instanceId = value; }
        }

        /// <summary>
        /// Time of generation
        /// </summary>
        public DateTime Time
        {
            get { return _time; }
            set { _time = value; }
        }

        /// <summary>
        /// Inforamtion to be displayed
        /// </summary>
        public string Information
        {
            get { return _information; }
            set { _information = value; }
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
