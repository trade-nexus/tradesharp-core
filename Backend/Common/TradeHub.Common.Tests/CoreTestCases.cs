using NUnit.Framework;
using TradeHub.Common.Core.Utility;

namespace TradeHub.Common.Tests
{
    [TestFixture]
    class CoreTestCases
    {
        [Test]
        public void SetUp()
        {
            
        }

        [TearDown]
        public void Close()
        {
            
        }

        [Test]
        [Category("Unit")]
        public void IdGeneratorTestCase()
        {
            var idOne = ApplicationIdGenerator.NextId();
            var idTwo = ApplicationIdGenerator.NextId();
            var idThree = ApplicationIdGenerator.NextId();

            Assert.AreEqual("A00", idOne, "Uniquely generated ID One");
            Assert.AreEqual("A01", idTwo, "Uniquely generated ID Two");
            Assert.AreEqual("A02", idThree, "Uniquely generated ID Three");
        }
    }
}
