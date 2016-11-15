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


﻿namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// For each opening transaction a position is created which also carries exit values
    /// and maintenance margin.
    /// </summary>
    public class Position
    {
        private long _quantity;
        private Security _security;
        private decimal _exitValue;
        private bool _isOpen;
        private string _provider;
        private decimal _avgBuyPrice;
        private decimal _avgSellPrice;
        private decimal _price;
        private PositionType _positionType;

        public Position()
        {
            
        }


        public decimal Price
        {
            get { return _price; }
            set { _price = value; }
        }

        public decimal AvgSellPrice
        {
            get { return _avgSellPrice; }
            set { _avgSellPrice = value; }
        }

        public decimal AvgBuyPrice
        {
            get { return _avgBuyPrice; }
            set { _avgBuyPrice = value; }
        }
        public long Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }
        
        public Security Security
        {
            get { return _security; }
            set { _security = value; }
        }

        public decimal ExitValue
        {
            get { return _exitValue; }
            set { _exitValue = value; }
        }

        public bool IsOpen
        {
            get { return _isOpen; }
            set { _isOpen=value; }
        }
        
        public string Provider
        {
            get { return _provider; }
            set { _provider = value; }
        }

        public PositionType Type
        {
            get { return _positionType; }
            set { _positionType = value; }
        }

        public override string ToString()
        {
            return "Provider: " + Provider + " | Symbol: " + Security.Symbol+
                   " | Position Type: " + Type + " | Quantity: " + Quantity +" | Price: " + _price+ " | AvgBuyPrice: " + _avgBuyPrice+
                   " | AvgSellPrice: " + _avgSellPrice + " | Open: " + IsOpen;
        }
    }



}

