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
using NUnit.Framework;
using TradeHub.StrategyRunner.ApplicationController.Domain;
using TradeHub.StrategyRunner.ApplicationController.Service;

namespace TradeHub.StrategyRunner.ApplicationController.Tests.Unit
{
    [TestFixture]
    public class OptimizationManagerTestCases
    {
        private OptimizationManagerBruteForce _optimizationManager;

        [SetUp]
        public void SetUp()
        {
            _optimizationManager = new OptimizationManagerBruteForce();
        }

        [Test]
        [Category("Unit")]
        public void IterateParametersTestCase()
        {
            List<object[]> tempList = new List<object[]>();
            
            // Possible Combinations
            object[] ctorArgsZero = new object[] { "1", "OPEN", "5", "Close", "0.3" };
            object[] ctorArgsOne = new object[] { "1", "OPEN", "6", "Close", "0.3" };
            object[] ctorArgsTwo = new object[] { "2", "OPEN", "5", "Close", "0.3" };
            object[] ctorArgsThree = new object[] { "2", "OPEN", "6", "Close", "0.3" };
            
            // Add to temporary list
            tempList.Add(ctorArgsZero);
            tempList.Add(ctorArgsOne);
            tempList.Add(ctorArgsTwo);
            tempList.Add(ctorArgsThree);

            // Create conditional parameters info
            Tuple<int, string, string>[] info = new Tuple<int, string, string>[]
                {
                    new Tuple<int, string, string>(0, "2", "1"),
                    new Tuple<int, string, string>(2, "6", "1")
                };

            // Get possible combinations from code
            _optimizationManager.CreateCtorCombinations(ctorArgsZero.Clone() as object[], info);

            // Verify
            Assert.AreEqual(tempList.Count, _optimizationManager.CtorArguments.Count, "Number of all Possible ctor iterations");
            Assert.IsTrue(ContainsValue(ctorArgsZero, _optimizationManager.CtorArguments), "Ctor values for Iteration Zero");
            Assert.IsTrue(ContainsValue(ctorArgsZero, _optimizationManager.CtorArguments), "Ctor values for Iteration One");
            Assert.IsTrue(ContainsValue(ctorArgsZero, _optimizationManager.CtorArguments), "Ctor values for Iteration Two");
            Assert.IsTrue(ContainsValue(ctorArgsZero, _optimizationManager.CtorArguments), "Ctor values for Iteration Three");
        }

        /// <summary>
        /// Checks if the value is already added in given list
        /// </summary>
        /// <param name="newValue">Value to verfiy</param>
        /// <param name="localMap">Local map to check for given value</param>
        private bool ContainsValue(object[] newValue, List<object[]> localMap)
        {
            foreach (object[] objects in localMap)
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    if (!newValue[i].Equals(objects[i]))
                    {
                        break;
                    }

                    if (i == objects.Length - 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
