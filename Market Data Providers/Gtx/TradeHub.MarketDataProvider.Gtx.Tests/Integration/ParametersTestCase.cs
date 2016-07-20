using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.Common.Fix.Infrastructure;

namespace TradeHub.MarketDataProvider.Gtx.Tests.Integration
{
    [TestFixture]
    public class ParametersTestCase
    {
        [Test]
        public void ReadFixSettings()
        {
            // Get parameter values
            var settings = ReadFixSettingsFile.GetSettings(AppDomain.CurrentDomain.BaseDirectory + @"\Config\GtxFIXSettings.txt");
            
            Assert.IsTrue(settings!=null, "Settings File Read");
            Assert.IsTrue(settings.Count.Equals(21), "Parameters Count");
        }
    }
}
