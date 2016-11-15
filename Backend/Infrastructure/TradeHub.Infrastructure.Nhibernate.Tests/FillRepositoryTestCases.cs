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
using NHibernate;
using NUnit.Framework;
using Spring.Context;
using Spring.Context.Support;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories;

namespace TradeHub.Infrastructure.Nhibernate.Tests
{
    public class FillRepositoryTestCases
    {
        private IFillRepository _fillRepository;
        private IApplicationContext ctx;
        private ISessionFactory sessionFactory;


        [SetUp]
        public void Setup()
        {
            ctx = ContextRegistry.GetContext();
            sessionFactory = ContextRegistry.GetContext()["NHibernateSessionFactory"] as ISessionFactory;
            _fillRepository = ContextRegistry.GetContext()["FillRepository"] as IFillRepository;
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        [Category("Integration")]
        public void PersistFill()
        {
            Fill fill = new Fill(new Security() { Isin = "123", Symbol = "AAPL" }, "BlackWood", "123");
            fill.ExecutionPrice = 100;
            fill.CummalativeQuantity = 100;
            fill.LeavesQuantity = 100;
            fill.ExecutionSize = 100;
            fill.ExecutionId = "asdfgfcx";
            fill.OrderId = "123";
            fill.ExecutionType = ExecutionType.Fill;
            _fillRepository.AddUpdate(fill);
            Fill getPersistedFill = _fillRepository.FindBy("asdfgfcx");
            AssertFillFields(fill,getPersistedFill);
        }
        
        /// <summary>
        /// verify all the fields of Fill
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="recevied"></param>
        private void AssertFillFields(Fill actual, Fill recevied)
        {
            Assert.AreEqual(actual.ExecutionId,recevied.ExecutionId);
            Assert.AreEqual(actual.AverageExecutionPrice, recevied.AverageExecutionPrice);
            Assert.AreEqual(actual.CummalativeQuantity, recevied.CummalativeQuantity);
            Assert.AreEqual(actual.ExecutionPrice, recevied.ExecutionPrice);
            Assert.AreEqual(actual.ExecutionSide, recevied.ExecutionSide);
            Assert.AreEqual(actual.ExecutionSize, recevied.ExecutionSize);
            Assert.AreEqual(actual.ExecutionType, recevied.ExecutionType);
            Assert.AreEqual(actual.OrderId, recevied.OrderId);
            Assert.AreEqual(actual.LeavesQuantity, recevied.LeavesQuantity);
        }
    }
}
