using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Spring.Context.Support;

namespace TradeHub.StrategyRunner.UserInterface.OptimizedStatsModule
{
    public class OptimizedStatsModule : IModule
    {
        /// <summary>
        /// Prism Region Handler
        /// </summary>
        private IRegionViewRegistry _regionViewRegistry;

        public void Initialize()
        {
            var context = ContextRegistry.GetContext();
            _regionViewRegistry.RegisterViewWithRegion("OptimizationStatsRegion", () => context.GetObject("OptimizationStatsView"));
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="registry"></param>
        public OptimizedStatsModule(IRegionViewRegistry registry)
        {
            _regionViewRegistry = registry;
        }
    }
}
