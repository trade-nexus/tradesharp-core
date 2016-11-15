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
using AForge;
using TraceSourceLogger;
using TradeHub.Optimization.Genetic.FitnessFunction;
using TradeHub.Optimization.Genetic.Tests.Application.HelperFunctions;

namespace TradeHub.Optimization.Genetic.Tests.Application.Utility
{
    public class MathimaticalFitnessFunction: OptimizationFunction4D
    {
        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="rangeW">Specifies W variable's range.</param>
        /// <param name="rangeX">Specifies X variable's range.</param>
        /// <param name="rangeY">Specifies Y variable's range.</param>
        /// <param name="rangeZ">Specifies Z variable's range.</param>
        public MathimaticalFitnessFunction(Range rangeW, Range rangeX, Range rangeY, Range rangeZ)
            : base(rangeW, rangeX, rangeY, rangeZ)
        {
        }

        #region Overrides of OptimizationFunction4D

        /// <summary>
        /// Function to optimize.
        /// </summary>
        /// <param name="w">Function W input value.</param>
        /// <param name="x">Function X input value.</param>
        /// <param name="y">Function Y input value.</param>
        /// <param name="z">Function Z input value.</param>
        /// <returns>Returns function output value.</returns>
        /// <remarks>The method should be overloaded by inherited class to
        /// specify the optimization function.</remarks>
        public override double OptimizationFunction(double w, double x, double y, double z)
        {
            double result = 0;

            double wUserValue = RangeCasting.ConvertValueToUserDefinedRange(w, 0.1);
            double xUserValue = RangeCasting.ConvertValueToUserDefinedRange(x, 0.01);
            double yUserValue = RangeCasting.ConvertValueToUserDefinedRange(y, 0.01);
            double zUserValue = RangeCasting.ConvertValueToUserDefinedRange(z, 0.1);

            // Calculate result
            result = ((wUserValue * zUserValue) + (xUserValue * yUserValue)) / (xUserValue * zUserValue);

            // Log Info
            Logger.Info("W:" + wUserValue, "MathimaticalFitnessFunction", "FitnessFunction");
            Logger.Info("X:" + xUserValue, "MathimaticalFitnessFunction", "FitnessFunction");
            Logger.Info("Y:" + yUserValue, "MathimaticalFitnessFunction", "FitnessFunction");
            Logger.Info("Z:" + zUserValue, "MathimaticalFitnessFunction", "FitnessFunction");
            Logger.Info("Ouput:" + result, "MathimaticalFitnessFunction", "FitnessFunction");

            // Print Info
            Console.WriteLine("1st Parameter: " + wUserValue);
            Console.WriteLine("2nd Parameter: " + xUserValue);
            Console.WriteLine("3rd Parameter: " + yUserValue);
            Console.WriteLine("4th Parameter: " + zUserValue);
            Console.WriteLine("Ouput:     " + result);

            // Return result
            return result;
        }

        #endregion
    }
}
