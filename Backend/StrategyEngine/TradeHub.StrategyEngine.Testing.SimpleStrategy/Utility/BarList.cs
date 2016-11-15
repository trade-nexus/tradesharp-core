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
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.StrategyEngine.Testing.SimpleStrategy.Utility
{
    public class BarList
    {
        private Bar[] _barArray;
        private int _index;
        private int _size;
        private string _emaPriceType;
        private int _lastBarIndex;

        public int ElemntCount
        {
            get { return this._barArray.Count(); }
        }

        public int WriteIndex
        {
            get { return this._index; }
            set { this._index = value; }
        }

        public int LastBarIndex
        {
            get { return this._lastBarIndex; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="size">Size of Bar array</param>
        /// <param name="emaPriceType">Price Type to be used for EMA calculations</param>
        public BarList(int size, string emaPriceType)
        {
            _size = size;
            _emaPriceType = emaPriceType;
            _barArray = new Bar[_size];
            _index = 0;
        }

        /// <summary>
        /// Adds the bar to the Array
        /// </summary>
        /// <param name="bar"></param>
        public void Add(Bar bar)
        {
            _barArray[_index % _size] = bar;
            if (++this._index > _size - 1)
            {
                _index = 0;
            }
            _lastBarIndex = _index - 1;
        }

        /// <summary>
        /// Removes the Bar from the list
        /// </summary>
        public void RemoveLast()
        {
            if (--_index < 0)
            {
                _index = 0;
            }
            _barArray[_index % _size] = null;
            _lastBarIndex = _index - 1;
        }

        /// <summary>
        /// Gets the element at specified location in the List
        /// </summary>
        /// <param name="index"></param>
        public Bar ElementAt(int index)
        {
            return _barArray[index];
        }

        /// <summary>
        /// Finds the sum of the Array for the required length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public decimal Sum(int length)
        {
            try
            {
                decimal sum = 0;
                int nextElementIndex = _index - 1;

                for (int i = 0; i < length; i++)
                {
                    if (nextElementIndex < 0)
                    {
                        nextElementIndex = _size - 1;
                    }
                    sum += BarPrice(_barArray[nextElementIndex]);
                    nextElementIndex--;
                }
                return sum;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), "TechnicalIndicators.Utilities", "Sum");
                return 0;
            }

        }

        /// <summary>
        /// Returns the returns the required price from the Bar
        /// </summary>
        /// <param name="bar"></param>
        public decimal BarPrice(Bar bar)
        {
            try
            {
                decimal price = 0;

                switch (_emaPriceType)
                {
                    case Constants.EmaPriceType.OPEN:
                        price = bar.Open;
                        break;
                    case Constants.EmaPriceType.HIGH:
                        price = bar.High;
                        break;
                    case Constants.EmaPriceType.LOW:
                        price = bar.Low;
                        break;
                    case Constants.EmaPriceType.CLOSE:
                        price = bar.Close;
                        break;
                }
                return price;
            }
            catch (Exception exception)
            {
                Logger.Error(exception.ToString(), "TechnicalIndicators.Utilities", "BarPrice");
                return 0;
            }
        }

        /// <summary>
        /// Checks if there is a NULL element in the list
        /// </summary>
        /// <returns></returns>
        public bool IsNull()
        {
            bool isNull = _barArray.Any(bar => bar == null);

            return isNull;
        }

        /// <summary>
        /// Gets price of all bars in the BarList according to the specified price type into a decimal Array
        /// </summary>
        /// <param name="barPriceType"> </param>
        public decimal[] GetBarPrices(string barPriceType)
        {
            var barPrices = new decimal[_size];
            for (int i = 0; i < _size; i++)
            {
                switch (barPriceType)
                {
                    case Constants.EmaPriceType.OPEN:
                        barPrices[i] = _barArray[i].Open;
                        break;
                    case Constants.EmaPriceType.HIGH:
                        barPrices[i] = _barArray[i].High;
                        break;
                    case Constants.EmaPriceType.LOW:
                        barPrices[i] = _barArray[i].Low;
                        break;
                    case Constants.EmaPriceType.CLOSE:
                        barPrices[i] = _barArray[i].Close;
                        break;
                }
            }
            return barPrices;
        }
    }
}
