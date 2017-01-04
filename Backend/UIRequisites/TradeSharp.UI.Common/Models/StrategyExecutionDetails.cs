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


using System.Collections.ObjectModel;
using System.ComponentModel;
using TradeHub.Common.Core.Constants;

namespace TradeSharp.UI.Common.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class StrategyExecutionDetails : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// List of all the Order detials during the current session
        /// </summary>
        private ObservableCollection<OrderDetails> _orderDetailsList;

        private string _key;
        private int _executed;
        private int _buyCount;
        private int _sellCount;

        /// <summary>
        /// Avg price for all the Buy orders till now
        /// </summary>
        private decimal _avgBuyPrice;

        /// <summary>
        /// Avg Price for all the Sell orders till now
        /// </summary>
        private decimal _avgSellPrice;

        /// <summary>
        /// Profit and Loss for the traded shares
        /// </summary>
        private decimal _profit;

        #endregion

        #region Properties

        public string Key
        {
            get { return _key; }
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged("Key");
                }
            }
        }

        public int Executed
        {
            get { return _executed; }
            set
            {
                if (_executed != value)
                {
                    _executed = value;
                    OnPropertyChanged("Executed");
                }
            }
        }

        public int BuyCount
        {
            get { return _buyCount; }
            set
            {
                if (_buyCount != value)
                {
                    _buyCount = value;
                    OnPropertyChanged("BuyCount");
                }
            }
        }

        public int SellCount
        {
            get { return _sellCount; }
            set
            {
                if (_sellCount != value)
                {
                    _sellCount = value;
                    OnPropertyChanged("SellCount");
                }
            }
        }

        /// <summary>
        /// List of all the Order detials during the current session
        /// </summary>
        public ObservableCollection<OrderDetails> OrderDetailsList
        {
            get { return _orderDetailsList; }
            set
            {
                _orderDetailsList = value;
                OnPropertyChanged("OrderDetailsList");
            }
        }

        /// <summary>
        /// Avg price for all the Buy orders till now
        /// </summary>
        public decimal AvgBuyPrice
        {
            get { return _avgBuyPrice; }
            set
            {
                _avgBuyPrice = value;
                OnPropertyChanged("AvgBuyPrice");
            }
        }

        /// <summary>
        /// Avg Price for all the Sell orders till now
        /// </summary>
        public decimal AvgSellPrice
        {
            get { return _avgSellPrice; }
            set
            {
                _avgSellPrice = value;
                OnPropertyChanged("AvgSellPrice");
            }
        }

        /// <summary>
        /// Profitl and Loss for the trades shares
        /// </summary>
        public decimal Profit
        {
            get { return _profit; }
            set
            {
                _profit = value;
                OnPropertyChanged("Profit");
            }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public StrategyExecutionDetails()
        {
            // Initialize Objects
            OrderDetailsList = new ObservableCollection<OrderDetails>();
        }

        /// <summary>
        /// Adds new order details to the local map
        /// </summary>
        /// <param name="orderDetails"></param>
        public void AddOrderDetails(OrderDetails orderDetails)
        {
            // Counts are to be incremented if incoming order detail is for execution
            if (orderDetails.Status.Equals(OrderStatus.EXECUTED) || orderDetails.Status.Equals(OrderStatus.PARTIALLY_EXECUTED))
            {
                Executed++;

                if (orderDetails.Side.Equals(OrderSide.COVER))
                {
                    if (BuyCount < SellCount)
                    {
                        AvgBuyPrice = ((orderDetails.Price * orderDetails.Quantity) + (AvgBuyPrice * BuyCount))
                                                      / (BuyCount += orderDetails.Quantity);
                        //BuyCount += orderDetails.Quantity;
                    }
                    else
                    {
                        AvgSellPrice = ((orderDetails.Price * orderDetails.Quantity) + (AvgSellPrice * SellCount))
                                                      / (SellCount += orderDetails.Quantity);
                        //SellCount += orderDetails.Quantity;
                    }
                }
                else if (orderDetails.Side.Equals(OrderSide.BUY))
                {
                    AvgBuyPrice = ((orderDetails.Price * orderDetails.Quantity) + (AvgBuyPrice * BuyCount))
                                                      / (BuyCount += orderDetails.Quantity);
                    //BuyCount += orderDetails.Quantity;
                }
                else
                {
                    AvgSellPrice = ((orderDetails.Price * orderDetails.Quantity) + (AvgSellPrice * SellCount))
                                                      / (SellCount += orderDetails.Quantity);
                    //SellCount += orderDetails.Quantity;
                }

                // Update PnL as per current executions
                Profit = (AvgSellPrice*SellCount) - (AvgBuyPrice*BuyCount);
            }

            // Add to local Map
            _orderDetailsList.Add(orderDetails);
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
