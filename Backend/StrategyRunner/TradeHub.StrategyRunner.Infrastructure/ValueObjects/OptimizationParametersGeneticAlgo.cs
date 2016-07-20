using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains info for the Strategy to be optimized using Genetic Algorithm
    /// </summary>
    public class OptimizationParametersGeneticAlgo
    {
        /// <summary>
        /// Constructor arguments to be used for the given custom strategy
        /// </summary>
        private object[] _ctorArguments;

        /// <summary>
        /// Holds reference for user selected custom strategy
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Contains info for the parameters to be used for Genetic Optimization
        /// </summary>
        private Dictionary<int, Tuple<string, Type>> _geneticAlgoParameters;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="ctorArguments">Constructor Arguments to be used</param>
        /// <param name="strategyType">Type of custom strategy</param>
        /// <param name="geneticAlgoParameters">Optimization parameters for Genetic Algo</param>
        public OptimizationParametersGeneticAlgo(object[] ctorArguments, Type strategyType, Dictionary<int, Tuple<string, Type>> geneticAlgoParameters)
        {
            _ctorArguments = ctorArguments;
            _strategyType = strategyType;
            _geneticAlgoParameters = geneticAlgoParameters;
        }

        /// <summary>
        /// Constructor arguments to be used for the given custom strategy
        /// </summary>
        public object[] CtorArguments
        {
            get { return _ctorArguments; }
        }

        /// <summary>
        /// Holds reference for user selected custom strategy
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
        }

        /// <summary>
        /// Contains info for the parameters to be used for Genetic Optimization
        /// </summary>
        public Dictionary<int, Tuple<string, Type>> GeneticAlgoParameters
        {
            get { return _geneticAlgoParameters; }
        }
    }
}
