using System;
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
