using System;
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
