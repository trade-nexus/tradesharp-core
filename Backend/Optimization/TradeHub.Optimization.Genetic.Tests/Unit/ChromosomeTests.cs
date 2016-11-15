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
using AForge.Genetic;
using NUnit.Framework;

namespace TradeHub.Optimization.Genetic.Tests.Unit
{
    [TestFixture]
    public class ChromosomeTests
    {
        [Test]
        public void TestCrossoverOnePoint()
        {
            SimpleStockTraderChromosome chromosome1 = new SimpleStockTraderChromosome(new Range[] { new Range(1, 5), new Range(2, 3) });
            SimpleStockTraderChromosome chromosome2 = new SimpleStockTraderChromosome(new Range[] { new Range(1, 6), new Range(2, 5)});
            SimpleStockTraderChromosome clone = (SimpleStockTraderChromosome)chromosome1.Clone();

            //When xover point is 1
            clone.SinglePointCrossover(chromosome2,1);
            Assert.AreEqual(clone.Values[0],chromosome1.Values[0]);
            Assert.AreEqual(clone.Values[1], chromosome2.Values[1]);
            //Assert.AreEqual(clone.Values[2], chromosome2.Values[2]);

            //When xover point is 0
            clone = (SimpleStockTraderChromosome)chromosome1.Clone();
            clone.SinglePointCrossover(chromosome2, 0);
            Assert.AreEqual(clone.Values[0], chromosome2.Values[0]);
            Assert.AreEqual(clone.Values[1], chromosome2.Values[1]);
            //Assert.AreEqual(clone.Values[2], chromosome2.Values[2]);

            //When xover point is 2
            clone = (SimpleStockTraderChromosome)chromosome1.Clone();
            clone.SinglePointCrossover(chromosome2, 2);
            Assert.AreEqual(clone.Values[0], chromosome1.Values[0]);
            Assert.AreEqual(clone.Values[1], chromosome1.Values[1]);
           // Assert.AreEqual(clone.Values[2], chromosome2.Values[2]);
            
        }
    }
}
