using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Spring.Context.Support;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.Repositories;

namespace TradeHub.Infrastructure.Nhibernate.Tests.Integration
{
    /// <summary>
    /// Persistence repository testcases.
    /// </summary>
    public class PersistenceRepositoryTestCases
    {
        private IPersistRepository<object> _persistRepository;

        [SetUp]
        public void Setup()
        {
            _persistRepository=ContextRegistry.GetContext()["PersistRepository"] as IPersistRepository<object>;
        }

        [Test]
        [Category("Integration")]
        public void PersistStrategyObject()
        {
            Strategy strategy=new Strategy();
            strategy.Name = "StockTrader";
            strategy.StartDateTime = DateTime.Now;
            _persistRepository.AddUpdate(strategy);
        }
    }
}
