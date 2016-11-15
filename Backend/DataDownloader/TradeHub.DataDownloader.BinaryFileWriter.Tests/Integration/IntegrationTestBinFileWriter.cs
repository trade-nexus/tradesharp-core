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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.DataDownloader.BinaryFileWriter.Tests.Integration
{
    [TestFixture]
    public class IntegrationTestBinFileWriter
    {
        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void TestWriteToBinFile()
        {
            var fileWriterBin=new FileWriterBinany();
            var tick = new Tick
                {
                    AskSize = 10,
                    BidPrice = 20,
                    BidSize = 2,
                    Security = new Security {Symbol = "AAPL"},
                    MarketDataProvider = "BlackWood",
                    AskPrice = 30,
                    DateTime = DateTime.Now,
                    LastPrice = 30,
                    LastSize = 2
                };
            fileWriterBin.Write(tick);
            Assert.AreEqual(true,
                            Directory.Exists("BlackWood\\AAPL\\TICK\\" +
                                             DateTime.Now.Year.ToString(CultureInfo.InvariantCulture) + "\\" +
                                             DateTime.Now.Month.ToString(CultureInfo.InvariantCulture)));
            Bar bar = new Bar(new Security {Symbol = "IBM"}, "BlackWood", "", DateTime.Now)
                {Close = 201, Open = 200, Low = 240, High = 300, Volume = 10};
            fileWriterBin.Write(bar);
            Assert.AreEqual(true,
                            Directory.Exists("BlackWood\\IBM\\BAR\\" +
                                             DateTime.Now.Year.ToString(CultureInfo.InvariantCulture) + "\\" +
                                             DateTime.Now.Month.ToString(CultureInfo.InvariantCulture)));
            string path = "BlackWood\\IBM\\BAR\\" +
                          DateTime.Now.Year.ToString(CultureInfo.InvariantCulture) + "\\" +
                          DateTime.Now.Month.ToString(CultureInfo.InvariantCulture) + "\\"  +DateTime.Now.ToString("yyyyMMdd")+".obj";
            Assert.AreEqual(true, File.Exists(path));
            Bar lastBar = ReadData(path);
            Assert.AreEqual(bar.DateTime,lastBar.DateTime);
        }

        /// <summary>
        /// Reads The Last Element of The file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Bar ReadData(string path)
        {
            var list = new List<Bar>();

            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var bFormatter = new BinaryFormatter();
                while (fileStream.Position != fileStream.Length)
                {
                    list.Add((Bar)bFormatter.Deserialize(fileStream));
                }
            }
            return list[list.Count-1];
        }
    }
}
