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


using TradeHub.Common.Core.Constants;

namespace TradeSharp.UI.Common.ValueObjects
{
    /// <summary>
    /// Contains complete bar information
    /// </summary>
    public class BarParameters
    {
        /// <summary>
        /// Bar format e.g. TIME
        /// </summary>
        private string _format;

        /// <summary>
        /// Bar price type e.g. ASK, LAST, BID
        /// </summary>
        private string _priceType;

        /// <summary>
        /// Pip size to be used in creating Bar entries
        /// </summary>
        private decimal _pipSize;

        /// <summary>
        /// Bar length e.g. 60 Seconds
        /// </summary>
        private decimal _barLength;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public BarParameters()
        {
            _barLength = 60;
            _pipSize = 0.0001M;
            _format = BarFormat.TIME;
            _priceType = BarPriceType.LAST;
        }

        #region Properties

        /// <summary>
        /// Bar format e.g. TIME
        /// </summary>
        public string Format
        {
            get { return _format; }
            set
            {
                _format = value; 
            }
        }

        /// <summary>
        /// Bar price type e.g. ASK, LAST, BID
        /// </summary>
        public string PriceType
        {
            get { return _priceType; }
            set
            {
                _priceType = value; 
            }
        }

        /// <summary>
        /// Pip size to be used in creating Bar entries
        /// </summary>
        public decimal PipSize
        {
            get { return _pipSize; }
            set
            {
                _pipSize = value; 
            }
        }

        /// <summary>
        /// Bar length e.g. 60 Seconds
        /// </summary>
        public decimal BarLength
        {
            get { return _barLength; }
            set
            {
                _barLength = value; 
            }
        }

        #endregion
    }
}
