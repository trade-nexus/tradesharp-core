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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Optimization.Genetic.Tests.Application.HelperFunctions
{
    public static class RangeCasting
    {
        /// <summary>
        /// Converts input values to appropariate AForge.Range values
        /// </summary>
        public static double ConvertInputToValidRangeValues(double value, double incrementLevel)
        {
            const double smallestValue = 0.0000000000000001; // 16 Decimal places
            double multiplyingFactor = 1;

            string[] multiplyingFactorStringValue = incrementLevel.ToString(CultureInfo.InvariantCulture.NumberFormat).Split('.');

            // Get Multiplying Factor
            if (multiplyingFactorStringValue.Length > 1)
            {
                // Add Zeros
                for (int i = 1; i <= multiplyingFactorStringValue[1].Length; i++)
                {
                    multiplyingFactor *= 10;
                }

                multiplyingFactor *= Convert.ToInt32(multiplyingFactorStringValue[1]);
            }

            // return value in the appropariate AForge.Range
            return (multiplyingFactor * value) * smallestValue;
        }

        /// <summary>
        /// Convert values to User defined range
        /// </summary>
        public static double ConvertValueToUserDefinedRange(double value, double incrementLevel)
        {
            double effectiveValue = 1;
            double multiplyingFactor = 1;

            string[] effectiveStringValue = value.ToString("F16", CultureInfo.InvariantCulture.NumberFormat).Split('.');
            string[] multiplyingFactorStringValue = incrementLevel.ToString(CultureInfo.InvariantCulture.NumberFormat).Split('.');

            // Get Orignal value
            effectiveValue = Convert.ToDouble(effectiveStringValue[1]);

            // Get Multiplying Factor
            if (multiplyingFactorStringValue.Length > 1)
            {
                // Add Zeros
                for (int i = 1; i <= multiplyingFactorStringValue[1].Length; i++)
                {
                    multiplyingFactor *= 10;
                }

                multiplyingFactor *= Convert.ToInt32(multiplyingFactorStringValue[1]);
            }

            // return value in the appropariate User defined Range
            return (effectiveValue / multiplyingFactor);
        }
    }
}
