using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge;
using TradeHub.Optimization.Genetic.FitnessFunction;
using TradeHub.Optimization.Genetic.HelperFunctions;
using TradeHub.Optimization.Genetic.Interfaces;

namespace TradeHub.Optimization.Genetic.FitnessFunctionImplementation
{
    /// <summary>
    /// Fitness function required for optimizing "StockTrader" Strategy
    /// </summary>
    public class StockTraderFitnessFunction : GeneticOptimization
    {
        /// <summary>
        /// Holds reference of the Strategy Executor to optimize user strategy
        /// </summary>
        private readonly IStrategyExecutor _strategyExecutor;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="strategyExecutor"> </param>
        public StockTraderFitnessFunction(IStrategyExecutor strategyExecutor)
        {
            _strategyExecutor = strategyExecutor;
        }

        #region Overrides of OptimizationFunction4D

        /// <summary>
        /// Function to optimize.
        /// </summary>
        /// <remarks>The method should be overloaded by inherited class to
        /// specify the optimization function.</remarks>
        public override double OptimizationFunction(double[] values)
        {
            double result = 0;
            // Calculate result
            result = _strategyExecutor.ExecuteStrategy(values);
            // Return result
            return result;
        }

        #endregion
    }
}
