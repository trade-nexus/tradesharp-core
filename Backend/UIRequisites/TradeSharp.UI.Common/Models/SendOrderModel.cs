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
using TradeSharp.UI.Common.Constants;

namespace TradeSharp.UI.Common.Models
{
    public class SendOrderModel : INotifyPropertyChanged
    {
        private decimal _buyPrice;
        private decimal _sellPrice;
        private decimal _triggerPrice;

        private int _size;

        private OrderType _type;

        private Security _security;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SendOrderModel()
        {
            _buyPrice = default(decimal);
            _sellPrice = default(decimal);
            _triggerPrice = default(decimal);

            _size = default(int);

            _type = OrderType.Market;
        }

        #region Properties

        /// <summary>
        /// Order Buy Price
        /// </summary>
        public decimal BuyPrice
        {
            get { return _buyPrice; }
            set
            {
                _buyPrice = value; 
                OnPropertyChanged("BuyPrice");
            }
        }

        /// <summary>
        /// Order Sell Price
        /// </summary>
        public decimal SellPrice
        {
            get { return _sellPrice; }
            set
            {
                _sellPrice = value;
                OnPropertyChanged("SellPrice");
            }
        }

        /// <summary>
        /// Order Trigger Price
        /// </summary>
        public decimal TriggerPrice
        {
            get { return _triggerPrice; }
            set
            {
                _triggerPrice = value;
                OnPropertyChanged("TriggerPrice");
            }
        }

        /// <summary>
        /// Type of Order
        /// </summary>
        public OrderType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
        }

        /// <summary>
        /// Order Quantity
        /// </summary>
        public int Size
        {
            get { return _size; }
            set
            {
                _size = value;
                OnPropertyChanged("Size");
            }
        }

        /// <summary>
        /// Contains symbol information
        /// </summary>
        public Security Security
        {
            get { return _security; }
            set
            {
                _security = value;
                OnPropertyChanged("Security");
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
