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
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.StrategyRunner.Infrastructure.Entities;

namespace TradeHub.StrategyRunner.Infrastructure.Tests
{
    [TestFixture]
    public class StatisticsTests
    {
        [Test]
        [Category("Unit")]
        public void RiskCalculationTests()
        {
            Statistics statistics=new Statistics("A00");
            Order order=new Order(OrderExecutionProvider.SimulatedExchange);
            Execution execution;
            for (int i = 0; i < 10; i++)
            {
                //1st trade 
                Fill fill1 = new Fill(new Security() {Symbol = "ERX"}, OrderExecutionProvider.SimulatedExchange, "1",
                    DateTime.Now);
                fill1.ExecutionPrice = 35.5m;
                fill1.ExecutionSize = 40;
                fill1.ExecutionSide = OrderSide.BUY;
                execution = new Execution(fill1, order);
                statistics.UpdateCalulcationsOnExecution(execution);

                Fill fill2 = new Fill(new Security() {Symbol = "ERX"}, OrderExecutionProvider.SimulatedExchange, "1",
                    DateTime.Now);
                fill2.ExecutionPrice = 36m;
                fill2.ExecutionSize = 40;
                fill2.ExecutionSide = OrderSide.SELL;
                execution = new Execution(fill2, order);
                statistics.UpdateCalulcationsOnExecution(execution);

                //2nd trade
                Fill fill3 = new Fill(new Security() {Symbol = "ERX"}, OrderExecutionProvider.SimulatedExchange, "1",
                    DateTime.Now);
                fill3.ExecutionPrice = 36.5m;
                fill3.ExecutionSize = 40;
                fill3.ExecutionSide = OrderSide.BUY;
                execution = new Execution(fill3, order);
                statistics.UpdateCalulcationsOnExecution(execution);

                Fill fill4 = new Fill(new Security() {Symbol = "ERX"}, OrderExecutionProvider.SimulatedExchange, "1",
                    DateTime.Now);
                fill4.ExecutionPrice = 36.23m;
                fill4.ExecutionSize = 40;
                fill4.ExecutionSide = OrderSide.SELL;
                execution = new Execution(fill4, order);
                statistics.UpdateCalulcationsOnExecution(execution);

                //3rd trade
                Fill fill5 = new Fill(new Security() {Symbol = "ERX"}, OrderExecutionProvider.SimulatedExchange, "1",
                    DateTime.Now);
                fill5.ExecutionPrice = 37.2m;
                fill5.ExecutionSize = 40;
                fill5.ExecutionSide = OrderSide.BUY;
                execution = new Execution(fill5, order);
                statistics.UpdateCalulcationsOnExecution(execution);

                Fill fill6 = new Fill(new Security() {Symbol = "ERX"}, OrderExecutionProvider.SimulatedExchange, "1",
                    DateTime.Now);
                fill6.ExecutionPrice = 37.3m;
                fill6.ExecutionSize = 40;
                fill6.ExecutionSide = OrderSide.SELL;
                execution = new Execution(fill6, order);
                statistics.UpdateCalulcationsOnExecution(execution);

                //4th trade
                Fill fill7 = new Fill(new Security() {Symbol = "ERX"}, OrderExecutionProvider.SimulatedExchange, "1",
                    DateTime.Now);
                fill7.ExecutionPrice = 38m;
                fill7.ExecutionSize = 20;
                fill7.ExecutionSide = OrderSide.BUY;
                execution = new Execution(fill7, order);
                statistics.UpdateCalulcationsOnExecution(execution);

                Fill fill8 = new Fill(new Security() {Symbol = "ERX"}, OrderExecutionProvider.SimulatedExchange, "1",
                    DateTime.Now);
                fill8.ExecutionPrice = 37.8m;
                fill8.ExecutionSize = 20;
                fill8.ExecutionSide = OrderSide.SELL;
                execution = new Execution(fill8, order);
                statistics.UpdateCalulcationsOnExecution(execution);

                Assert.AreEqual(4.440576923m, Math.Round(statistics.GetRisk(), 9));
                statistics.ResetAllValues();
            }
        }

        [Test]
        public void TestUtilityFunctionValue()
        {
            Statistics statistics = new Statistics("A00");
            Order order = new Order(OrderExecutionProvider.SimulatedExchange);
            Execution execution;
            string[] files = File.ReadAllLines(@"C:\Users\Muhammad Bilal\Desktop\DATA_2014-08-28\stats_06-52-55-PM.txt");
            for (int i = 0; i < files.Length; i++)
            {
                string[] temp = files[i].Trim().Split('|');
                Fill fill1 = new Fill(new Security() {Symbol = "ERX"}, OrderExecutionProvider.SimulatedExchange, "1",
                    DateTime.Now);
                fill1.ExecutionPrice = decimal.Parse(temp[4].Split(':')[1].Trim());
                string[] size = temp[3].Split(' ');
                fill1.ExecutionSize = int.Parse(temp[3].Split(' ')[3].Trim());
                fill1.ExecutionSide = temp[2].Split(':')[1].Trim();
                execution = new Execution(fill1, order);
                statistics.UpdateCalulcationsOnExecution(execution);
            }
            Console.WriteLine("Risk="+  statistics.GetRisk());
            Assert.AreEqual(4.440576923m,statistics.GetRisk());
            statistics.ResetAllValues();
        }
    }
}
