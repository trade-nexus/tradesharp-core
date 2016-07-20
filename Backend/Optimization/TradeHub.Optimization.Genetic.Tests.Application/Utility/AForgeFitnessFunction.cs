using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge;
using TraceSourceLogger;
using TradeHub.Optimization.Genetic.FitnessFunction;
using TradeHub.Optimization.Genetic.Tests.Application.HelperFunctions;

namespace TradeHub.Optimization.Genetic.Tests.Application.Utility
{
    public class AForgeFitnessFunction: OptimizationFunction4D
    {
        /// <summary>
        /// Holds reference of the strategy to be optimized
        /// </summary>
        private readonly TestStrategyExecutor _strategyExecutor;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="strategyExecutor"> </param>
        /// <param name="rangeW">Specifies W variable's range.</param>
        /// <param name="rangeX">Specifies X variable's range.</param>
        /// <param name="rangeY">Specifies Y variable's range.</param>
        /// <param name="rangeZ">Specifies Z variable's range.</param>
        public AForgeFitnessFunction( TestStrategyExecutor strategyExecutor, Range rangeW, Range rangeX, Range rangeY, Range rangeZ)
            : base(rangeW, rangeX, rangeY, rangeZ)
        {
            _strategyExecutor = strategyExecutor;
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

            //double wUserValue = ConvertValueToUserDefinedRange(w, 0.1);
            //double xUserValue = ConvertValueToUserDefinedRange(x, 0.0001);
            //double yUserValue = ConvertValueToUserDefinedRange(y, 0.1);
            //double zUserValue = ConvertValueToUserDefinedRange(z, 0.001);

            // Calculate result
            result = _strategyExecutor.ExecuteStrategy(w, x, y, z);

            //Logger.Info("ALPHA:   " + wUserValue, "Optimization", "FitnessFunction");
            //Logger.Info("BETA:    " + xUserValue, "Optimization", "FitnessFunction");
            //Logger.Info("GAMMA:   " + yUserValue, "Optimization", "FitnessFunction");
            //Logger.Info("EPSILON: " + zUserValue, "Optimization", "FitnessFunction");
            //Logger.Info("PNL:     " + result, "Optimization", "FitnessFunction");

            // Return result
            return result;
        }

        #endregion

        /// <summary>
        /// Convert values to User defined range
        /// </summary>
        private double ConvertValueToUserDefinedRange(double value, double incrementLevel)
        {
            double effectiveValue = 1;
            double multiplyingFactor = 1;

            string[] effectiveStringValue = value.ToString("F16", CultureInfo.InvariantCulture.NumberFormat).Split('.');
            string[] multiplyingFactorStringValue = incrementLevel.ToString(CultureInfo.InvariantCulture.NumberFormat).Split('.');

            // Get Orignal value
            effectiveValue = Convert.ToDouble(effectiveStringValue[1]);

            // Get Multiplying Factor
            if (multiplyingFactorStringValue.Length > 1)
            {
                // Add Zeros
                for (int i = 1; i <= multiplyingFactorStringValue[1].Length; i++)
                {
                    multiplyingFactor *= 10;
                }

                multiplyingFactor *= Convert.ToInt32(multiplyingFactorStringValue[1]);
            }

            // return value in the appropariate User defined Range
            return (effectiveValue / multiplyingFactor);
        }
    
    }
}
