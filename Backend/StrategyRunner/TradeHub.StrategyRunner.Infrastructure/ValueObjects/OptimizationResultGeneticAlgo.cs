using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains optimization results for strategy execution through Genetic Algo
    /// </summary>
    public class OptimizationResultGeneticAlgo
    {
        /// <summary>
        /// Fitness achieved
        /// </summary>
        private double _fitnessValue;

        /// <summary>
        /// Contains optimized values of the required parameters
        /// Key = Parameter Index
        /// Value = Optimized Value
        /// </summary>
        private Dictionary<int, double> _optimizedParameters;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="fitnessValue">Fitness achieved</param>
        /// <param name="optimizedParameters">Contains optimized values of the required parameters</param>
        public OptimizationResultGeneticAlgo(double fitnessValue, Dictionary<int, double> optimizedParameters)
        {
            _optimizedParameters = optimizedParameters;
            _fitnessValue = fitnessValue;
        }

        /// <summary>
        /// Fitness achieved
        /// </summary>
        public double FitnessValue
        {
            get { return _fitnessValue; }
        }

        /// <summary>
        /// Contains optimized values of the required parameters
        /// Key = Parameter Index
        /// Value = Optimized Value
        /// </summary>
        public Dictionary<int, double> OptimizedParameters
        {
            get { return _optimizedParameters; }
        }

    }
}
