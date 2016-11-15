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


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains info to identify the selected user strategy
    /// </summary>
    public class SelectedStrategy
    {
        /// <summary>
        /// Unique key to identify strategy
        /// </summary>
        private string _key;

        /// <summary>
        /// Symbol on which the strategy is executing
        /// </summary>
        private string _symbol;

        /// <summary>
        /// Brief info related to strategy parameters
        /// </summary>
        private string _briefInfo;

        /// <summary>
        /// Indicates whether the strategy is running/stopped
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// Unique key to identify strategy
        /// </summary>
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        /// <summary>
        /// Symbol on which the strategy is executing
        /// </summary>
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        /// <summary>
        /// Brief info related to strategy parameters
        /// </summary>
        public string BriefInfo
        {
            get { return _briefInfo; }
            set { _briefInfo = value; }
        }

        /// <summary>
        /// Indicates whether the strategy is running/stopped
        /// </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }
        
        /// <summary>
        /// ToString override for the SelectedStrategy.cs
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder= new StringBuilder();

            stringBuilder.Append("Selected Strategy :: ");
            stringBuilder.Append("Key: " + _key);
            stringBuilder.Append(" | Symbol: " + _symbol);
            stringBuilder.Append(" | Brief Info: " + _briefInfo);

            return stringBuilder.ToString();
        }
    }
}
