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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.StrategyRunner.ApplicationController.Service;
using TradeHub.StrategyRunner.Infrastructure.ValueObjects;

namespace TradeHub.StrategyRunner.ApplicationController.Tests.Integration
{
    [TestFixture]
    public class StrategyExecutorTestCases
    {
        [SetUp]
        public void Setup()
        {
            
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        [Test]
        public void RunStrategyMultipleTimes()
        {
            object[] _ctorArguments = new object[]
                {
                    (Int32) 100, (decimal) 1.5, (uint) 40, "ERX", (decimal) 45, (float) 0.2, (decimal) 0.005,
                    (float) 0.005, (decimal) 0.005, "9:30", "9:30", (decimal) 10, (decimal) 0.04, "SDOT", (float) 0.006,
                    (decimal) 0.01, (decimal) 0.01,
                    "SimulatedExchange", "SimulatedExchange"
                };
            //string[] file = File.ReadAllLines(@"C:\Users\Muhammad Bilal\Downloads\matlab_singlepoint_data.csv");
            //for (int i = 0; i < file.Length; i++)
            //{
            //    string[] param = file[i].Split(',');
                
            //    object alpha = param[0];
            //    object beta = param[1];
            //    object gamma = param[2];
            //    object espilon = param[3];

            //    StrategyController controller=new StrategyController();
            //    Assembly assembly = Assembly.LoadFrom(@"C:\Users\Muhammad Bilal\Desktop\StockTrader - Copy\StockTrader.Common.dll");
            //    LoadStrategy strategy = new LoadStrategy(assembly);
            //    controller.LoadUserStrategy(strategy);
            //    controller.InitializeUserStrategy(new InitializeStrategy());
            //}
            

        }
    }
}
