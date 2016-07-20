using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Optimera;
using TraceSourceLogger;

namespace TradeHub.Optimization.Genetic.Tests.Application.Utility
{
    /// <summary>
    /// Implements Optimera's Fitness Interface
    /// </summary>
    public class OptimeraFitnessFunction : IOptimisable
    {
        private double _alpha;
        private double _beta;
        private double _gamma;
        private double _epsilon;

        /// <summary>
        /// Holds reference of the strategy to be optimized
        /// </summary>
        private readonly TestStrategyExecutor _strategyExecutor;

        private readonly int _numberOfParameters;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="numberOfParameters">Number of parameters to be used for Optimization</param>
        /// <param name="strategyExecutor">Strategy Executor to be used</param>
        public OptimeraFitnessFunction(int numberOfParameters, TestStrategyExecutor strategyExecutor)
        {
            _strategyExecutor = strategyExecutor;
            _numberOfParameters = numberOfParameters;
        }

        #region Implementation of IOptimisable

        /// <summary>
        /// Returns the number of parameters to be used to Optimization
        /// </summary>
        /// <returns></returns>
        public int NumberOfParameters()
        {
            return _numberOfParameters;
        }

        /// <summary>
        /// Performs the required operations to calculates fitness
        /// </summary>
        /// <param name="normalizedParameters"></param>
        /// <returns></returns>
        public double Fitness(double[] normalizedParameters)
        {
            
            double result = 0;

            // Calculate result
            result = _strategyExecutor.ExecuteStrategy(normalizedParameters[0], normalizedParameters[1],
                                                       normalizedParameters[2], normalizedParameters[3]);

            Logger.Info("ALPHA:   " + normalizedParameters[0], "Optimization", "FitnessFunction");
            Logger.Info("BETA:    " + normalizedParameters[1], "Optimization", "FitnessFunction");
            Logger.Info("GAMMA:   " + normalizedParameters[2], "Optimization", "FitnessFunction");
            Logger.Info("EPSILON: " + normalizedParameters[3], "Optimization", "FitnessFunction");
            Logger.Info("PNL:     " + result, "Optimization", "FitnessFunction");

            // Return result
            return result;
        }

        /// <summary>
        /// Creates a Deep Clone for the Fitness Class
        /// </summary>
        /// <returns></returns>
        public object DeepClone()
        {
            // Create new object
            OptimeraFitnessFunction clone = new OptimeraFitnessFunction(_numberOfParameters, _strategyExecutor)
                {
                    _alpha = this._alpha,
                    _beta = this._beta,
                    _gamma = this._gamma,
                    _epsilon = this._epsilon
                };

            // Return Clone
            return clone;
        }

        #endregion
    }

}
