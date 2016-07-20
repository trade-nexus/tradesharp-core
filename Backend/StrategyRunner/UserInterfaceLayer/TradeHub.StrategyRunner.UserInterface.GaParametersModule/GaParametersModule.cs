using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Spring.Context.Support;

namespace TradeHub.StrategyRunner.UserInterface.GaParametersModule
{
    public class GaParametersModule : IModule
    {
        /// <summary>
        /// Prism Region Handler
        /// </summary>
        private IRegionViewRegistry _regionViewRegistry;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="registry"></param>
        public GaParametersModule(IRegionViewRegistry registry)
        {
            _regionViewRegistry = registry;
        }

        #region Implementation of IModule

        public void Initialize()
        {
            var context = ContextRegistry.GetContext();
            _regionViewRegistry.RegisterViewWithRegion("GaParametersRegion", () => context.GetObject("GaParametersView"));
        }

        #endregion
    }
}
