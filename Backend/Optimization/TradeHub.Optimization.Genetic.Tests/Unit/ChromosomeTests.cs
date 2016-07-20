using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge;
using AForge.Genetic;
using NUnit.Framework;

namespace TradeHub.Optimization.Genetic.Tests.Unit
{
    [TestFixture]
    public class ChromosomeTests
    {
        [Test]
        public void TestCrossoverOnePoint()
        {
            SimpleStockTraderChromosome chromosome1 = new SimpleStockTraderChromosome(new Range[] { new Range(1, 5), new Range(2, 3) });
            SimpleStockTraderChromosome chromosome2 = new SimpleStockTraderChromosome(new Range[] { new Range(1, 6), new Range(2, 5)});
            SimpleStockTraderChromosome clone = (SimpleStockTraderChromosome)chromosome1.Clone();

            //When xover point is 1
            clone.SinglePointCrossover(chromosome2,1);
            Assert.AreEqual(clone.Values[0],chromosome1.Values[0]);
            Assert.AreEqual(clone.Values[1], chromosome2.Values[1]);
            //Assert.AreEqual(clone.Values[2], chromosome2.Values[2]);

            //When xover point is 0
            clone = (SimpleStockTraderChromosome)chromosome1.Clone();
            clone.SinglePointCrossover(chromosome2, 0);
            Assert.AreEqual(clone.Values[0], chromosome2.Values[0]);
            Assert.AreEqual(clone.Values[1], chromosome2.Values[1]);
            //Assert.AreEqual(clone.Values[2], chromosome2.Values[2]);

            //When xover point is 2
            clone = (SimpleStockTraderChromosome)chromosome1.Clone();
            clone.SinglePointCrossover(chromosome2, 2);
            Assert.AreEqual(clone.Values[0], chromosome1.Values[0]);
            Assert.AreEqual(clone.Values[1], chromosome1.Values[1]);
           // Assert.AreEqual(clone.Values[2], chromosome2.Values[2]);
            
        }
    }
}
