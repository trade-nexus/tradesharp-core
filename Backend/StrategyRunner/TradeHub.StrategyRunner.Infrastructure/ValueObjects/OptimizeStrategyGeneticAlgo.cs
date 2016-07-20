using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains info for the parameters to be used when initiating optimization
    /// </summary>
    public class OptimizeStrategyGeneticAlgo
    {
        /// <summary>
        /// Constructor arguments to use
        /// </summary>
        private object[] _ctorArgs;

        /// <summary>
        /// Contains info for the parameters to be used for optimization
        /// </summary>
        private SortedDictionary<int,GeneticAlgoParameters> _optimzationParameters;
        
        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        private Type _strategyType;


        /// <summary>
        /// Iterations of the GA to be run
        /// </summary>
        private int _iterations;

        //create population size
        private int _populationSize;

        /// <summary>
        /// Argument Constrcutor
        /// </summary>
        /// <param name="strategyType">Type of custom strategy used</param>
        /// <param name="ctorArgs">Constructor arguments to be used for given strategy</param>
        /// <param name="optimzationParameters">Parameters to be used for optimizing the strategy</param>
        public OptimizeStrategyGeneticAlgo(Type strategyType, object[] ctorArgs, SortedDictionary<int, GeneticAlgoParameters> optimzationParameters,int iterations,int populationSize)
        {
            _strategyType = strategyType;
            _ctorArgs = ctorArgs;
            _optimzationParameters = optimzationParameters;
            _iterations = iterations;
            _populationSize = populationSize;
        }

        /// <summary>
        /// Constructor arguments to use
        /// </summary>
        public object[] CtorArgs
        {
            get { return _ctorArgs; }
        }

        /// <summary>
        /// Contains info for the parameters to be used for optimization
        /// </summary>
        public SortedDictionary<int, GeneticAlgoParameters> OptimzationParameters
        {
            get { return _optimzationParameters; }
        }

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
        }

        /// <summary>
        /// Contains info for Population Size
        /// </summary>
        public int PopulationSize
        {
            get { return _populationSize; }
            set { _populationSize = value; }
        }

        /// <summary>
        /// Contains info for Iterations.
        /// </summary>
        public int Iterations
        {
            get { return _iterations; }
            set { _iterations = value; }
        }

    }
}
