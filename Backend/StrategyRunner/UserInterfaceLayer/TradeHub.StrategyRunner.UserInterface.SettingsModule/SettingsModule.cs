using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Spring.Context.Support;

namespace TradeHub.StrategyRunner.UserInterface.SettingsModule
{
    public class SettingsModule: IModule
    {
        /// <summary>
        /// Prism Region Handler
        /// </summary>
        private IRegionViewRegistry _regionViewRegistry;

        public void Initialize()
        {
            var context = ContextRegistry.GetContext();
            _regionViewRegistry.RegisterViewWithRegion("SettingsPanelRegion", () => context.GetObject("SettingsPanelView"));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="registry"></param>
        public SettingsModule(IRegionViewRegistry registry)
        {
            _regionViewRegistry = registry;
        }
    }
}
