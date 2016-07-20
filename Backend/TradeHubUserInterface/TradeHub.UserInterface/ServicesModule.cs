using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using TradeHub.UserInterface.ServicesModule.View;

namespace TradeHub.UserInterface.ServicesModule
{
   public class ServicesModule:IModule
    {
        /// <summary>
        /// Prism Region Handler
        /// </summary>
        private IRegionViewRegistry _regionViewRegistry;

        public void Initialize()
        {
            _regionViewRegistry.RegisterViewWithRegion("MajorView",()=>new TradeHubServicesView());
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="registry"></param>
        public ServicesModule(IRegionViewRegistry registry)
        {
            _regionViewRegistry = registry;
        }
    }
}
