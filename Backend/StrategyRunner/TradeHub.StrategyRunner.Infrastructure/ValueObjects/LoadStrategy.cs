using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains details of the user strategy to be loaded
    /// </summary>
    public class LoadStrategy
    {
        private Assembly _strategyAssembly;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public LoadStrategy(Assembly strategyAssembly)
        {
            _strategyAssembly = strategyAssembly;
        }

        /// <summary>
        /// Gets/Sets Custom Strategy Assembly
        /// </summary>
        public Assembly StrategyAssembly
        {
            get { return _strategyAssembly; }
            set { _strategyAssembly = value; }
        }
    }
}
