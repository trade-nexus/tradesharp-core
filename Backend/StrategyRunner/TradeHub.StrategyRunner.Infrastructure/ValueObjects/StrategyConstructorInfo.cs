using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains Contructor details for the selected user strategy
    /// </summary>
    public class StrategyConstructorInfo
    {
        /// <summary>
        /// Holds parameter details for the custom strategy
        /// </summary>
        private ParameterInfo[] _parameterInfo;

        /// <summary>
        /// Holds reference of the User selected strategy Type(Contains TradeHubStrategy implementation)
        /// </summary>
        private Type _strategyType;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="parameterInfo">Contains Constructor Parameter details</param>
        /// <param name="strategyType">Type from user selected assembly containing TradeHubStrategy implementation</param>
        public StrategyConstructorInfo(ParameterInfo[] parameterInfo, Type strategyType)
        {
            _strategyType = strategyType;
            ParameterInfo = parameterInfo;
        }

        /// <summary>
        /// Gets/Sets parameter details for the custom strategy
        /// </summary>
        public ParameterInfo[] ParameterInfo
        {
            get { return _parameterInfo; }
            set { _parameterInfo = value; }
        }

        /// <summary>
        /// Holds reference of the User selected strategy Type(Contains TradeHubStrategy implementation)
        /// </summary>
        public Type StrategyType
        {
            get { return _strategyType; }
            set { _strategyType = value; }
        }

    }
}
