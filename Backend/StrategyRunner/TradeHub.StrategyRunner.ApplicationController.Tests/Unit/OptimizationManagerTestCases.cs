using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TradeHub.StrategyRunner.ApplicationController.Domain;
using TradeHub.StrategyRunner.ApplicationController.Service;

namespace TradeHub.StrategyRunner.ApplicationController.Tests.Unit
{
    [TestFixture]
    public class OptimizationManagerTestCases
    {
        private OptimizationManagerBruteForce _optimizationManager;

        [SetUp]
        public void SetUp()
        {
            _optimizationManager = new OptimizationManagerBruteForce();
        }

        [Test]
        [Category("Unit")]
        public void IterateParametersTestCase()
        {
            List<object[]> tempList = new List<object[]>();
            
            // Possible Combinations
            object[] ctorArgsZero = new object[] { "1", "OPEN", "5", "Close", "0.3" };
            object[] ctorArgsOne = new object[] { "1", "OPEN", "6", "Close", "0.3" };
            object[] ctorArgsTwo = new object[] { "2", "OPEN", "5", "Close", "0.3" };
            object[] ctorArgsThree = new object[] { "2", "OPEN", "6", "Close", "0.3" };
            
            // Add to temporary list
            tempList.Add(ctorArgsZero);
            tempList.Add(ctorArgsOne);
            tempList.Add(ctorArgsTwo);
            tempList.Add(ctorArgsThree);

            // Create conditional parameters info
            Tuple<int, string, string>[] info = new Tuple<int, string, string>[]
                {
                    new Tuple<int, string, string>(0, "2", "1"),
                    new Tuple<int, string, string>(2, "6", "1")
                };

            // Get possible combinations from code
            _optimizationManager.CreateCtorCombinations(ctorArgsZero.Clone() as object[], info);

            // Verify
            Assert.AreEqual(tempList.Count, _optimizationManager.CtorArguments.Count, "Number of all Possible ctor iterations");
            Assert.IsTrue(ContainsValue(ctorArgsZero, _optimizationManager.CtorArguments), "Ctor values for Iteration Zero");
            Assert.IsTrue(ContainsValue(ctorArgsZero, _optimizationManager.CtorArguments), "Ctor values for Iteration One");
            Assert.IsTrue(ContainsValue(ctorArgsZero, _optimizationManager.CtorArguments), "Ctor values for Iteration Two");
            Assert.IsTrue(ContainsValue(ctorArgsZero, _optimizationManager.CtorArguments), "Ctor values for Iteration Three");
        }

        /// <summary>
        /// Checks if the value is already added in given list
        /// </summary>
        /// <param name="newValue">Value to verfiy</param>
        /// <param name="localMap">Local map to check for given value</param>
        private bool ContainsValue(object[] newValue, List<object[]> localMap)
        {
            foreach (object[] objects in localMap)
            {
                for (int i = 0; i < objects.Length; i++)
                {
                    if (!newValue[i].Equals(objects[i]))
                    {
                        break;
                    }

                    if (i == objects.Length - 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
