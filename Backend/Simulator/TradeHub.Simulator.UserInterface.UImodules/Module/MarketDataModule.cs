using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Spring.Context.Support;

namespace TradeHub.SimulatedExchange.UserInterface.UImodules.Module
{
    public class MarketDataModule:IModule
    {
        /// <summary>
        /// Prism Region Handler
        /// </summary>
        private IRegionViewRegistry _regionViewRegistry;

        public void Initialize()
        {
            /*Associate a view with a region, by registering a type. When the
            * region get's displayed this type will be resolved 
            * using the ServiceLocator into a concrete instance. 
            * The instance will be added to the Views collection of the region*/
            var context = ContextRegistry.GetContext();
            _regionViewRegistry.RegisterViewWithRegion("DataCenter", () => context.GetObject("DataView"));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="registry"></param>
        public MarketDataModule(IRegionViewRegistry registry)
        {
            _regionViewRegistry = registry;
        }
    }
}
