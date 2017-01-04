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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeSharp.UI.Common.Constants;

namespace TradeSharp.UI.Common.Models
{
    public class OrderDetails : INotifyPropertyChanged
    {
        private string _id;
        private Security _security;
        private string _side;
        private OrderType _type;
        private decimal _price;
        private decimal _stopPrice;
        private int _quantity;
        private DateTime _time;
        private string _status;
        private string _provider;

        /// <summary>
        /// Holds all related fills for the order
        /// </summary>
        private ObservableCollection<FillDetail> _fillDetails;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public OrderDetails(string providerName)
        {
            // Save reference
            _provider = providerName;

            // Initialze fields
            _time = DateTime.UtcNow;
            _status = OrderStatus.OPEN;
            _fillDetails = new ObservableCollection<FillDetail>();
        }

        #region Properties

        public string ID
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged("ID");
                }
            }
        }

        public string Side
        {
            get { return _side; }
            set
            {
                if (_side != value)
                {
                    _side = value;
                    OnPropertyChanged("Side");
                }
            }
        }

        public OrderType Type
        {
            get { return _type; }
            set
            {
                if (_type != value)
                {
                    _type = value;
                    OnPropertyChanged("Type");
                }
            }
        }

        public decimal Price
        {
            get { return _price; }
            set
            {
                if (_price != value)
                {
                    _price = value;
                    OnPropertyChanged("Price");
                }
            }
        }

        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged("Quantity");
                }
            }
        }

        public DateTime Time
        {
            get { return _time; }
            set
            {
                if (_time != value)
                {
                    _time = value;
                    OnPropertyChanged("Time");
                }
            }
        }

        /// <summary>
        /// Order Status represented as const static strings from TradeHub.Common.Core.Constants.OrderStatus class
        /// possible values are: CANCELLED, EXECUTED, OPEN, PARTIALLY_EXECUTED, REJECTED, SUBMITTED
        /// </summary>
        public string Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged("Status");
                }
            }
        }

        /// <summary>
        /// Stop price to be used with Stop/Stop Limit orders
        /// </summary>
        public decimal StopPrice
        {
            get { return _stopPrice; }
            set 
            {
                if (_stopPrice != value)
                {
                    _stopPrice = value;
                    OnPropertyChanged("StopPrice");
                }
            }
        }

        /// <summary>
        /// Holds all related fills for the order
        /// </summary>
        public ObservableCollection<FillDetail> FillDetails
        {
            get { return _fillDetails; }
            set 
            {
                if (_fillDetails != value)
                {
                    _fillDetails = value;
                    OnPropertyChanged("FillDetails");
                }
            }
        }

        /// <summary>
        /// Order execution provider name
        /// </summary>
        public string Provider
        {
            get { return _provider; }
            set
            {
                if (_provider != value)
                {
                    _provider = value;
                    OnPropertyChanged("Provider");
                }
            }
        }

        /// <summary>
        /// Contains Symbol information
        /// </summary>
        public Security Security
        {
            get { return _security; }
            set
            {
                if (_security != value)
                {
                    _security = value;
                    OnPropertyChanged("Security");
                }
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

        /// <summary>
        /// Provides basic execution info
        /// </summary>
        public string BasicExecutionInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            
            stringBuilder.Append(_security.Symbol);
            stringBuilder.Append(",");
            stringBuilder.Append(_id);
            stringBuilder.Append(",");
            stringBuilder.Append(_side);
            stringBuilder.Append(",");
            stringBuilder.Append(_quantity);
            stringBuilder.Append(",");
            stringBuilder.Append(_price);
            stringBuilder.Append(",");
            stringBuilder.Append(_status);
            stringBuilder.Append(",");
            stringBuilder.Append(_time);

            if (_fillDetails.Count > 0)
            {
                foreach (var fillDetail in _fillDetails)
                {
                    stringBuilder.Append(",");
                    stringBuilder.Append(fillDetail.FillPrice);
                    stringBuilder.Append(",");
                    stringBuilder.Append(fillDetail.FillDatetime);
                }
            }

            return stringBuilder.ToString();
        }
	}
}
