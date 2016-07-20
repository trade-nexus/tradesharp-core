using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using TradeHub.UserInterface.ComponentsModule.Views;

namespace TradeHub.UserInterface.ComponentsModule
{
    public class ComponentsModule:IModule
    {
         /// <summary>
        /// Prism Region Handler
        /// </summary>
        private IRegionViewRegistry _regionViewRegistry;

        public void Initialize()
        {
            _regionViewRegistry.RegisterViewWithRegion("ComponentsView", () => new ComponentsView());
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="registry"></param>
        public ComponentsModule(IRegionViewRegistry registry)
        {
            _regionViewRegistry = registry;
        }
    }
}
