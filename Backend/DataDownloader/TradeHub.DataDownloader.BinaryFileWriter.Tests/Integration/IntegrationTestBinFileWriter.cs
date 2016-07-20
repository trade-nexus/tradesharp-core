using System;
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
