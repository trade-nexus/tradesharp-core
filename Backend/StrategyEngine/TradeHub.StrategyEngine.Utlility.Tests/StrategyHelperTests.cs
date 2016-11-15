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
using TradeHub.StrategyEngine.Utlility.Services;

namespace TradeHub.StrategyEngine.Utlility.Tests
{
    [TestFixture]
    public class StrategyHelperTests
    {
        private DirectoryInfo _dir1 = null;
        private DirectoryInfo _dir2 = null;

        [SetUp]
        public void Setup()
        {
            _dir1 = null;
            _dir2 = null;
        }

        [TearDown]
        public void TearDown()
        {
            if (_dir1 != null)
            {
                _dir1.Delete();
            }
            if (_dir2 != null)
            {
                _dir2.Delete();
            }
        }

        [Test]
        [Category("Integration")]
        public void ValidAssemblyVerificationTest()
        {
            string assemblyPath =
                Path.GetFullPath(
                    @"~\..\..\..\..\TradeHub.StrategyEngine.Testing.SimpleStrategy\bin\Debug\TradeHub.StrategyEngine.Testing.SimpleStrategy.dll");

            Assert.IsTrue(StrategyHelper.ValidateStrategy(assemblyPath));
        }

        [Test]
        [Category("Integration")]
        public void InvalidAssemblyVerificationTest()
        {
            string assemblyPath =
                Path.GetFullPath(
                    @"~\..\..\..\..\TradeHub.StrategyEngine.Testing.SimpleStrategy\bin\Debug\TradeHub.Common.Core.dll");
            Assert.IsFalse(StrategyHelper.ValidateStrategy(assemblyPath));
        }

        [Test]
        [Category("Integration")]
        public void CopySytrategyAssemblyAfterVerificationTest()
        {
            string assemblyPath =
                Path.GetFullPath(
                    @"~\..\..\..\..\TradeHub.StrategyEngine.Testing.SimpleStrategy\bin\Debug\TradeHub.StrategyEngine.Testing.SimpleStrategy.dll");

            bool verified = StrategyHelper.ValidateStrategy(assemblyPath);
            bool copied = false;

            StrategyHelper.CopyAssembly(assemblyPath);
            var allStrategyNames = StrategyHelper.GetAllStrategiesName();

            foreach (string strategyName in allStrategyNames)
            {
                if (strategyName.Equals("TradeHub.StrategyEngine.Testing.SimpleStrategy"))
                {
                    copied = true;
                    break;
                }
            }

            Assert.IsTrue(verified);
            Assert.IsTrue(copied);
        }

        [Test]
        [Category("Integration")]
        public void DeleteSytrategyAssemblyFromSavedDirectoryTest()
        {
            bool deleted = true;

            //var folderName =
            //    StrategyHelper.GetStrategyFileName(
            //        @"~\..\..\..\..\TradeHub.StrategyEngine.Testing.SimpleStrategy\bin\Debug\TradeHub.StrategyEngine.Testing.SimpleStrategy.dll");

            var folderName = "TradeHub.StrategyEngine.Testing.SimpleStrategy";
            StrategyHelper.RemoveAssembly(folderName);

            var allStrategyNames = StrategyHelper.GetAllStrategiesName();

            foreach (string strategyName in allStrategyNames)
            {
                if (strategyName.Equals("TradeHub.StrategyEngine.Testing.SimpleStrategy"))
                {
                    deleted = false;
                    break;
                }
            }

            Assert.IsTrue(deleted);
        }

        [Test]
        [Category("Integration")]
        public void GetAllStrategiesNameTest()
        {
            _dir1 = Directory.CreateDirectory(DirectoryStructure.STRATEGY_LOCATION + "\\Strategy1");
            _dir2 = Directory.CreateDirectory(DirectoryStructure.STRATEGY_LOCATION + "\\Strategy2");
            List<string> strategies = StrategyHelper.GetAllStrategiesName();
            Assert.AreEqual(2, strategies.Count);
            Assert.AreEqual("Strategy1", strategies[0]);
            Assert.AreEqual("Strategy2", strategies[1]);
        }

        [Test]
        [Category("Integration")]
        public void GetStrategyPathTest()
        {
            _dir1 = Directory.CreateDirectory(DirectoryStructure.STRATEGY_LOCATION + "\\Strategy1");
            string path = StrategyHelper.GetStrategyPath("Strategy1");
            Assert.AreEqual(DirectoryStructure.STRATEGY_LOCATION + "\\Strategy1\\Strategy1.dll", path);
        }

        [Test]
        [Category("Integration")]
        public void GetAllStrategiesPathTest()
        {
            _dir1 = Directory.CreateDirectory(DirectoryStructure.STRATEGY_LOCATION + "\\Strategy1");
            var strategiesPaths = StrategyHelper.GetAllStrategiesPath();
            Assert.AreEqual(1, strategiesPaths.Count);
            Assert.AreEqual(DirectoryStructure.STRATEGY_LOCATION + "\\Strategy1\\Strategy1.dll", strategiesPaths[0]);
        }

        [Test]
        [Category("Integration")]
        public void GetParameterDetails_LoadAssembly_ReturnInfo_Successfull()
        {
            string assemblyPath =
                Path.GetFullPath(
                    @"~\..\..\..\..\TradeHub.StrategyEngine.Testing.SimpleStrategy\bin\Debug\TradeHub.StrategyEngine.Testing.SimpleStrategy.dll");

            // Get Class Type from assembly
            var classtype = StrategyHelper.GetStrategyClassType(assemblyPath);

            // Get parameters info
            var details = StrategyHelper.GetParameterDetails(classtype);

            Assert.AreEqual(details["shortEma"], typeof (int), "Short EMA");
            Assert.AreEqual(details["longEma"], typeof (int), "Long EMA");
            Assert.AreEqual(details["emaPriceType"], typeof (string), "emaPriceType");
            Assert.AreEqual(details["symbol"], typeof (string), "symbol");
            Assert.AreEqual(details["barLength"], typeof (decimal), "barLength");
            Assert.AreEqual(details["barFormat"], typeof (string), "barFormat");
            Assert.AreEqual(details["barPriceType"], typeof (string), "barPriceType");
            Assert.AreEqual(details["marketDataProvider"], typeof (string), "marketDataProvider");
            Assert.AreEqual(details["orderExecutionProvider"], typeof (string), "orderExecutionProvider");
        }

        [Test]
        [Category("Integration")]
        public void GetParameterDetails_LoadAssembly_ReturnInfo_Fail()
        {
            string assemblyPath =
                Path.GetFullPath(
                    @"~\..\..\..\..\TradeHub.StrategyEngine.Testing.SimpleStrategy\bin\Debug\TradeHub.StrategyEngine.Testing.SimpleStrategy.dll");

            // Get Class Type from assembly
            var classtype = StrategyHelper.GetStrategyClassType(assemblyPath);

            // Get parameters info
            var details = StrategyHelper.GetParameterDetails(classtype);

            Assert.AreNotEqual(details["shortEma"], typeof (Int64), "Short EMA");
            Assert.AreEqual(details["longEma"], typeof (int), "Long EMA");
            Assert.AreEqual(details["emaPriceType"], typeof (string), "emaPriceType");
            Assert.AreEqual(details["symbol"], typeof (string), "symbol");
            Assert.AreEqual(details["barLength"], typeof (decimal), "barLength");
            Assert.AreEqual(details["barFormat"], typeof (string), "barFormat");
            Assert.AreEqual(details["barPriceType"], typeof (string), "barPriceType");
            Assert.AreEqual(details["marketDataProvider"], typeof (string), "marketDataProvider");
            Assert.AreEqual(details["orderExecutionProvider"], typeof (string), "orderExecutionProvider");
        }

        [Test]
        [Category("Integration")]
        public void GetConstructorDetails_LoadAssembly_ReturnInfo_Successfull()
        {
            string assemblyPath =
                Path.GetFullPath(
                    @"~\..\..\..\..\TradeHub.StrategyEngine.Testing.SimpleStrategy\bin\Release\TradeHub.StrategyEngine.Testing.SimpleStrategy.dll");

            // Get Class Type from assembly
            var details = StrategyHelper.GetConstructorDetails(assemblyPath);

            Assert.AreEqual(details.Item2.Count(), 9, "Parameter Count");
        }
    }
}
