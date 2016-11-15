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
using NUnit.Framework;
using TradeHub.StrategyRunner.Infrastructure.Service;

namespace TradeHub.StrategyRunner.Infrastructure.Tests.Integration
{
    [TestFixture]
    public class LoadCustomStrategyTests
    {
        private string _assemblyName = @"TradeHub.StrategyEngine.Testing.SimpleStrategy.dll";

        [SetUp]
        public void Setup()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        public void GetCustomAttributesTestCase()
        {
            // Load Assembly file from the selected file
            Assembly assembly = Assembly.LoadFrom(_assemblyName);

            // Contains custom defined attributes in the given assembly
            Dictionary<int, Tuple<string, Type>> customAttributes = null;

            // Get Constructor information for the given assembly
            var strategyDetails = LoadCustomStrategy.GetConstructorDetails(assembly);

            if (strategyDetails != null)
            {
                // Get Strategy Type
                var strategyType = strategyDetails.Item1;

                // Get custom attributes from the given assembly
                customAttributes = LoadCustomStrategy.GetCustomAttributes(strategyType);
            }

            Assert.IsNotNull(strategyDetails, "Constructor Information for the given assembly was not found.");
            Assert.IsNotNull(customAttributes, "Custom attributes were not found in the given assembly.");
            Assert.AreEqual(3, customAttributes.Count, "Count of read custom attributes was not equal to expected value.");
        }
    }
}
