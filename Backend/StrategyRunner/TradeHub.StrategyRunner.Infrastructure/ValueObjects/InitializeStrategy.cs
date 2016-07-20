using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains info to initialize the selected strategy
    /// </summary>
    public class InitializeStrategy
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
        /// Holds reference of user selected custom strategy
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
            set { _strategyType = value; }
        }

        /// <summary>
        /// Holds constructor arguments. 
        /// Required to successfully initialize the given strategy assembly.
        /// </summary>
        public object[] CtorArguments
        {
            get { return _ctorArguments; }
            set { _ctorArguments = value; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="strategyType">User defined custom strategy</param>
        /// <param name="ctorArguments">Constructor arguments required to initialize given strategy</param>
        public InitializeStrategy(Type strategyType, object[] ctorArguments)
        {
            _strategyType = strategyType;
            _ctorArguments = ctorArguments;
        }
    }
}
