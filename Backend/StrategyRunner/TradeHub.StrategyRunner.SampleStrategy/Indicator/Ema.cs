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
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.StrategyRunner.SampleStrategy.Utility;

namespace TradeHub.StrategyRunner.SampleStrategy.Indicator
{
    /// <summary>
    /// 2EMA technical indicator
    /// </summary>
    public class EMA
    {
        private Type _oType = typeof(EMA);
        private BarList _barList;
        private int _shortEMA = 0;
        private int _longEMA = 0;
        private string _emaType;
        private decimal[] _ema;


        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="shortEMA"></param>
        /// <param name="longEMA"></param>
        /// <param name="emaType"></param>
        public EMA(int shortEMA, int longEMA, string emaType)
        {
            _shortEMA = shortEMA;
            _longEMA = longEMA;
            _emaType = emaType;
            _barList = new BarList(this._longEMA, emaType);
            _ema = new decimal[2] { 0, 0 };
        }

        /// <summary>
        /// Adds Bars to the List
        /// </summary>
        /// <param name="bar"></param>
        public void AddBars(Bar bar)
        {
            try
            {
                this._barList.Add(bar);
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _oType.FullName, "AddBars");
            }
        }

        /// <summary>
        /// Calculates Long and Short EMA and returns the values.
        /// </summary>
        /// <param name="bar"></param>
        public decimal[] GetEMA(Bar bar)
        {
            try
            {
                // Calculate Long EMA value
                _ema[0] = CalculateEMA(bar, this._longEMA, _ema[0]);
                // Calculate Short EMA value
                _ema[1] = CalculateEMA(bar, this._shortEMA, _ema[1]);
                return _ema;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), _oType.FullName, "GetEMA");
                return new decimal[2] { 0, 0 };
            }

        }

        /// <summary>
        /// Calcumates EMA
        /// </summary>
        private decimal CalculateEMA(Bar bar, int emaLength, decimal previousEMA)
        {
            try
            {
                // Calculate Alpha value
                decimal alpha = 2 / Convert.ToDecimal(emaLength + 1);
                // Get current bar price
                decimal currentPrice = _barList.BarPrice(bar);
                // Calculate EMA
                decimal ema = previousEMA == 0 ? currentPrice : GetEMA(previousEMA, alpha, currentPrice);

                return decimal.Round(ema, 6, MidpointRounding.AwayFromZero);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "CalculateEMA");
                return 0;
            }
        }

        public decimal GetEMA(decimal previousEMA, decimal alpha, decimal currentPrice)
        {
            try
            {
                return previousEMA + alpha * (currentPrice - previousEMA);
            }

            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "GetEMA");
                return 0;
            }
        }
    }
}
