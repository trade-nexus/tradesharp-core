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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.StrategyEngine.Utlility.Services;

namespace TradeHub.Optimization.Genetic.Tests.Application.Utility
{
    /// <summary>
    /// Responsible for Optimization using Brute Force
    /// </summary>
    public class BruteForceOptimization
    {
        private TestStrategyExecutor _strategyExecutor;

        /// <summary>
        /// Contians all possible Constructor Arguments combinations
        /// </summary>
        private List<object[]> _ctorArguments;

        /// <summary>
        /// Save constuctor parameter info for the selected strategy
        /// </summary>
        private System.Reflection.ParameterInfo[] _parmatersDetails;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public BruteForceOptimization(ParameterInfo[] parmatersDetails, TestStrategyExecutor strategyExecutor)
        {
            // Save info
            _parmatersDetails = parmatersDetails;
            _strategyExecutor = strategyExecutor;

            // Initialize
            _ctorArguments = new List<object[]>();
        }

        /// <summary>
        /// Performs iterations depending upon the range and parameter values
        /// </summary>
        public void ExecuteIterations()
        {
            // Execute all combinations
            foreach (object[] ctorArgument in _ctorArguments)
            {
                double result = 0;

                // Calculate result
                result = _strategyExecutor.ExecuteStrategy(Convert.ToDouble(ctorArgument[1].ToString()),
                                                              Convert.ToDouble(ctorArgument[14].ToString()),
                                                              Convert.ToDouble(ctorArgument[5].ToString()),
                                                              Convert.ToDouble(ctorArgument[6].ToString()));

                Logger.Info("ALPHA:   " + ctorArgument[1], "Optimization", "ExecuteIterations");
                Logger.Info("BETA:    " + ctorArgument[14], "Optimization", "ExecuteIterations");
                Logger.Info("GAMMA:   " + ctorArgument[5], "Optimization", "ExecuteIterations");
                Logger.Info("EPSILON: " + ctorArgument[6], "Optimization", "ExecuteIterations");
                Logger.Info("PNL:     " + result, "Optimization", "ExecuteIterations");

                //// Return result
                //return result;
            }
        }

        /// <summary>
        /// Creates all possible ctor combinations
        /// </summary>
        /// <param name="ctorArgs">ctor arguments to create combinations with</param>
        /// <param name="conditionalParameters">contains info for the conditional parameters</param>
        public void CreateCtorCombinations(object[] ctorArgs, Tuple<int, string, string>[] conditionalParameters)
        {
            try
            {
                var itemsCount = conditionalParameters.Length;
                // Get all posible optimizations
                GetAllIterations(ctorArgs.Clone() as object[], conditionalParameters, itemsCount - 1);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        /// <summary>
        /// Gets all possible combinations for the given parameters
        /// </summary>
        /// <param name="args">ctor arguments to create combinations with</param>
        /// <param name="conditionalParameters">contains info for the conditional parameters</param>
        /// <param name="conditionalIndex">index of conditional parameter to be used for iterations</param>
        private void GetAllIterations(object[] args, Tuple<int, string, string>[] conditionalParameters, int conditionalIndex)
        {
            try
            {
                // get index of parameter to be incremented
                int index = conditionalParameters[conditionalIndex].Item1;

                // Get end value for the parameter
                decimal endPoint;
                if (!decimal.TryParse(conditionalParameters[conditionalIndex].Item2, out endPoint))
                {
                    return;
                }

                // Get increment value to be used 
                decimal increment;
                if (!decimal.TryParse(conditionalParameters[conditionalIndex].Item3, out increment))
                {
                    return;
                }

                // Get Orignal Value
                decimal orignalValue = Convert.ToDecimal(args[index]);

                // Iterate through all combinations
                for (decimal i = 0; ; i += increment)
                {
                    // Modify parameter value
                    var parameter = orignalValue + i;

                    if (parameter > endPoint) break;

                    // Convert string value to required format
                    var value = StrategyHelper.GetParametereValue(parameter.ToString(), _parmatersDetails[index].ParameterType.Name);

                    // Update arguments array
                    args[index] = value;

                    // Check if the combination is already present
                    if (!ValueAdded(args, _ctorArguments, index))
                    {
                        // Add the updated arguments to local map
                        _ctorArguments.Add(args.Clone() as object[]);

                        // Get further iterations if 
                        if (conditionalIndex > 0)
                        {
                            GetAllIterations(args.Clone() as object[], conditionalParameters, conditionalIndex - 1);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        /// <summary>
        /// Checks if the value is already added in given list
        /// </summary>
        /// <param name="newValue">Value to verfiy</param>
        /// <param name="localMap">Local map to check for given value</param>
        /// <param name="index">Index on which to verify the value</param>
        private bool ValueAdded(object[] newValue, List<object[]> localMap, int index)
        {
            if (localMap.Count > 0)
            {
                var lastElement = localMap.Last();
                if (lastElement != null)
                {
                    if (lastElement[index].Equals(newValue[index]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
