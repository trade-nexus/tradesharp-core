using System;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TradeHub.Common.Core.DomainModels;

namespace TradeHub.DataDownloader.CsvFileWriter.Tests.Integration
{
    [TestFixture]
    public class IntegrationTestCsvFileWriter
    {
        [Test]
        public void TestWriteToFile()
        {
            var csvWriter = new FileWriterCsv();
            Bar bar = new Bar(new Security { Symbol = "IBM" }, "BlackWood", "",DateTime.Now) { Close = 201, Open = 200, Low = 240, High = 300, Volume = 10 };
            csvWriter.Write(bar);
            string path = "BlackWood" + "\\" + "IBM" + "\\" + "BAR" + "\\" +
            DateTime.Now.Year.ToString(CultureInfo.InvariantCulture) + "\\" +
            DateTime.Now.Month.ToString(CultureInfo.InvariantCulture);
            Assert.IsTrue(Directory.Exists(path));
            string lastLine = ReturnLastLine(path + "\\" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
            Assert.AreEqual(61,lastLine.Length);
        }

        private string ReturnLastLine(string path)
        {
           return File.ReadLines(path).Last(); 
        }
    }
}
