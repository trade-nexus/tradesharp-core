using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge;
using AForge.Genetic;
using AForge.Math.Random;
using NUnit.Framework;
using TradeHub.Optimization.Genetic.FitnessFunction;

namespace TradeHub.Optimization.Genetic.Tests.Unit
{
    [TestFixture]
    public class OptimizationFunction4DUnitTests
    {
        private Population _population;
        private CustomFitnessFunction _fitnessFunction;

        [SetUp]
        public void Setup()
        {
            //_fitnessFunction = new CustomFitnessFunction(new Range(1, 10), new Range(1, 10),
            //                                                    new Range(1, 10), new Range(1, 10));

            //// Set Fitness Mode
            //_fitnessFunction.Mode = OptimizationFunction4D.Modes.Maximization;

            //// create genetic population
            //_population = new Population(100, new BinaryChromosome(32), _fitnessFunction, new EliteSelection());

            //_population.CrossoverRate = 0.50;
            //_population.MutationRate = 0.25;
        }

        [TearDown]
        public void Close()
        {
            
        }

        /// <summary>
        /// Test case to calculate fitness for the custom defined "Fitness Function"
        /// </summary>
        [Test]
        [Category("Unit")]
        public void FitnessCalculationTest()
        {
            const int count = 35;

            for (int iterator = 0; iterator < count; iterator++)
            {
                _population.RunEpoch();
            }

            double[] paramterValues = _fitnessFunction.Translate(_population.BestChromosome);

            Assert.IsNotNull(paramterValues, "The parameters were not calculated properly: NULL");

            Assert.GreaterOrEqual(paramterValues[0], 9d, "1st parameter was not the best option");
            Assert.GreaterOrEqual(paramterValues[1], 1d, "2nd parameter was not the best option");
            Assert.GreaterOrEqual(paramterValues[2], 9d, "3rd parameter was not the best option");
            Assert.GreaterOrEqual(paramterValues[3], 1d, "4th parameter was not the best option");

            Console.WriteLine("1st Parameter: " + paramterValues[0]);
            Console.WriteLine("2nd Parameter: " + paramterValues[1]);
            Console.WriteLine("3rd Parameter: " + paramterValues[2]);
            Console.WriteLine("4th Parameter: " + paramterValues[3]);
        }

        [Test]
        public void TestRandomNumbers()
        {
            UniformGenerator generator=new UniformGenerator(new Range(0.0001f,0.011f),DateTime.Now.Millisecond);
            
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(generator.Next()+" ");
            }
        }
    }



    /// <summary>
    /// Provide basic fitness function for formula: ((a.d)+(b.c))/b.d
    /// </summary>
    public class CustomFitnessFunction : OptimizationFunction4D
    {
        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="rangeW">Specifies W variable's range.</param>
        /// <param name="rangeX">Specifies X variable's range.</param>
        /// <param name="rangeY">Specifies Y variable's range.</param>
        /// <param name="rangeZ">Specifies Z variable's range.</param>
        public CustomFitnessFunction(Range rangeW, Range rangeX, Range rangeY, Range rangeZ) : base(rangeW, rangeX, rangeY, rangeZ)
        {
        }

        #region Overrides of OptimizationFunction4D

        /// <summary>
        /// Function to optimize.
        /// </summary>
        /// <param name="w">Function W input value.</param>
        /// <param name="x">Function X input value.</param>
        /// <param name="y">Function Y input value.</param>
        /// <param name="z">Function Z input value.</param>
        /// <returns>Returns function output value.</returns>
        /// <remarks>The method should be overloaded by inherited class to
        /// specify the optimization function.</remarks>
        public override double OptimizationFunction(double w, double x, double y, double z)
        {
            double result = 0;

            // Calculate result
            result = ((w * z) + (x * y)) / (x * z);

            // Return result
            return result;
        }

        #endregion
    }
}
