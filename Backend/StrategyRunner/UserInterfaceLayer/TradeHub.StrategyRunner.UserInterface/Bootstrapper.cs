using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;  
using Microsoft.Practices.Prism.UnityExtensions;
using Spring.Context;
using Spring.Context.Support;
using TradeHub.StrategyRunner.UserInterface.Shell;

namespace TradeHub.StrategyRunner.UserInterface
{
    public class Bootstrapper : UnityBootstrapper
    {
        #region Overrides of Bootstrapper

        /// <summary>
        /// Initializing Application Shell.
        /// </summary>
        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }

        /// <summary>
        /// Creates Shell Object
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject CreateShell()
        {
            IApplicationContext context = ContextRegistry.GetContext();
            return (ApplicationShell)context.GetObject("Shell");
        }

        /// <summary>
        /// Add Modules to the Catalog
        /// </summary>
        protected override void ConfigureModuleCatalog()
        {
            //set logging path
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                              "\\TradeHub Logs\\Client";
            TraceSourceLogger.Logger.LogDirectory(path);
            base.ConfigureModuleCatalog();
            var moduleCatalog = (ModuleCatalog)ModuleCatalog;
            moduleCatalog.AddModule(typeof(SettingsModule.SettingsModule));
            moduleCatalog.AddModule(typeof(SearchModule.SearchModule));
            moduleCatalog.AddModule(typeof(StatsModule.StatsModule));
            moduleCatalog.AddModule(typeof(StrategyModule.StrategyModule));
            moduleCatalog.AddModule(typeof(ParametersModule.ParametersModule));
            moduleCatalog.AddModule(typeof(OptimizedStatsModule.OptimizedStatsModule));
            moduleCatalog.AddModule(typeof(GaParametersModule.GaParametersModule));
            moduleCatalog.AddModule(typeof(GaStatsModule.GaStatsModule));
        }

        #endregion
    }
}
