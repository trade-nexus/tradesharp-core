using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Spring.Context.Support;

namespace TradeHub.StrategyRunner.UserInterface.GaStatsModule
{
    public class GaStatsModule : IModule
    {
        /// <summary>
        /// PRISM region handler
        /// </summary>
        private IRegionViewRegistry _regionViewRegistry;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="registry"></param>
        public GaStatsModule(IRegionViewRegistry registry)
        {
            _regionViewRegistry = registry;
        }

        #region Implementation of IModule

        public void Initialize()
        {
            var context = ContextRegistry.GetContext();
            
            //Register Ga Stats View region
            _regionViewRegistry.RegisterViewWithRegion("GaStatsRegion", () => context.GetObject("GaStatsView"));
        }

        #endregion
    }
}
