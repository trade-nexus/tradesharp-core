using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains info for the parameters to be used when initiating optimization
    /// </summary>
    public class OptimizeStrategyBruteForce
    {
        /// <summary>
        /// Constructor arguments to use
        /// </summary>
        private object[] _ctorArgs;

        /// <summary>
        /// Parameters to use for creating iterations
        /// </summary>
        private Tuple<int, string, string>[] _conditionalParameters;

        /// <summary>
        /// Save constuctor parameter details for the selected strategy
        /// </summary>
        private readonly ParameterInfo[] _parmatersDetails;

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Constructor arguments to use
        /// </summary>
        public object[] CtorArgs
        {
            get { return _ctorArgs; }
            set { _ctorArgs = value; }
        }

        /// <summary>
        /// Parameters to use for creating iterations
        /// </summary>
        public Tuple<int, string, string>[] ConditionalParameters
        {
            get { return _conditionalParameters; }
            set { _conditionalParameters = value; }
        }

        /// <summary>
        /// Holds reference of user selected custom strategy
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
            set { _strategyType = value; }
        }

        /// <summary>
        /// Save constuctor parameter details for the selected strategy
        /// </summary>
        public ParameterInfo[] ParmatersDetails
        {
            get { return _parmatersDetails; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="ctorArgs">Constructor arguments to use</param>
        /// <param name="strategyType">Reference of user selected custom strategy</param>
        /// <param name="conditionalParameters">Parameters to use for creating iterations</param>
        /// <param name="parmatersDetails">Save constuctor parameter details for the selected strategy</param>
        public OptimizeStrategyBruteForce(object[] ctorArgs, Type strategyType, Tuple<int, string, string>[] conditionalParameters, ParameterInfo[] parmatersDetails)
        {
            _ctorArgs = ctorArgs;
            _strategyType = strategyType;
            _conditionalParameters = conditionalParameters;
            _parmatersDetails = parmatersDetails;
        }
    }
}
