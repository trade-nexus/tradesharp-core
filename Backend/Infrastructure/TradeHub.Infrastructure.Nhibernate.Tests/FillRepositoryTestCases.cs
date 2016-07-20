using System;
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
