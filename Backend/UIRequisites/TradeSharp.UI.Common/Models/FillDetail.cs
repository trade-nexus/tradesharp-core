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
using TradeHub.Common.Core.Constants;

namespace TradeSharp.UI.Common.Models
{
    /// <summary>
    /// Contains Order's Fill information
    /// </summary>
    public class FillDetail : INotifyPropertyChanged
    {
        /// <summary>
        /// Unique ID to identify Fill
        /// </summary>
        private string _fillId;

        /// <summary>
        /// Quantity filled in the current cycle
        /// </summary>
        private int _fillQuantity;

        /// <summary>
        /// Price for the filled quantity
        /// </summary>
        private decimal _fillPrice;

        /// <summary>
        /// Fill Time
        /// </summary>
        private DateTime _fillDatetime;

        /// <summary>
        /// Type of fill i.e. Fill, Partial
        /// </summary>
        private ExecutionType _fillType;

        #region Properties

        /// <summary>
        /// Unique ID to identify Fill
        /// </summary>
        public string FillId
        {
            get { return _fillId; }
            set
            {
                _fillId = value; 
                OnPropertyChanged("FillId");
            }
        }

        /// <summary>
        /// Quantity filled in the current cycle
        /// </summary>
        public int FillQuantity
        {
            get { return _fillQuantity; }
            set
            {
                _fillQuantity = value;
                OnPropertyChanged("FillQuantity");
            }
        }

        /// <summary>
        /// Price for the filled quantity
        /// </summary>
        public decimal FillPrice
        {
            get { return _fillPrice; }
            set
            {
                _fillPrice = value;
                OnPropertyChanged("FillPrice");
            }
        }

        /// <summary>
        /// Fill Time
        /// </summary>
        public DateTime FillDatetime
        {
            get { return _fillDatetime; }
            set
            {
                _fillDatetime = value; 
                OnPropertyChanged("FillDateTime");
            }
        }

        /// <summary>
        /// Type of fill i.e. Fill, Partial
        /// </summary>
        public ExecutionType FillType
        {
            get { return _fillType; }
            set
            {
                _fillType = value; 
                OnPropertyChanged("FillType");
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
