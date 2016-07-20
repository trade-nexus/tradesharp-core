using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains info for the strategy to be optimized using Brute Force
    /// </summary>
    public class OptimizationParametersBruteForce
    {
        /// <summary>
        /// Holds constructor arguments. 
        /// Required to successfully initialize the given strategy assembly.
        /// </summary>
        private object[] _ctorArguments;

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Contains info regarding the parameters
        /// </summary>
        private ParameterInfo[] _parameterDetails;

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
        }

        /// <summary>
        /// Holds constructor arguments. 
        /// Required to successfully initialize the given strategy assembly.
        /// </summary>
        public object[] CtorArguments
        {
            get { return _ctorArguments; }
        }

        /// <summary>
        /// Contains info regarding the parameters
        /// </summary>
        public ParameterInfo[] ParameterDetails
        {
            get { return _parameterDetails; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="strategyType">User defined custom strategy</param>
        /// <param name="ctorArguments">Constructor arguments required to initialize given strategy</param>
        /// <param name="parameterDetails">Contains info regarding the parameters</param>
        public OptimizationParametersBruteForce(Type strategyType, object[] ctorArguments, ParameterInfo[] parameterDetails)
        {
            _strategyType = strategyType;
            _ctorArguments = ctorArguments;
            _parameterDetails = parameterDetails;
        }
    }
}
