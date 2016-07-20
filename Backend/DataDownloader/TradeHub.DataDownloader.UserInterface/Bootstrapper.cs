using System;
using System.Windows;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Spring.Context;
using Spring.Context.Support;
using TradeHub.DataDownloader.UserInterface.Shell.Shell;

namespace TradeHub.DataDownloader.UserInterface
{
    /// <summary>
    /// Bootstraper Class is The Entery Point of Our UI layer.
    /// This class handover control to Prism.
    /// </summary>
    public class Bootstrapper : UnityBootstrapper
    {
        private readonly Type _oType = typeof (Bootstrapper);

        /// <summary>
        /// Initializing Application Shell.
        /// Need to run initialization steps to ensure that 
        /// the shell is ready to be displayed.
        /// </summary>
        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window) Shell;
            Application.Current.MainWindow.Show();
        }


        /// <summary>
        /// Creating Application Shell.
        /// Resolving Shell from Container 
        /// and returning it to parent class
        /// </summary>
        /// <returns></returns>
        protected override DependencyObject CreateShell()
        {
            IApplicationContext context = ContextRegistry.GetContext();
            return (ApplicationShell)context.GetObject("Shell");
            //return (ApplicationShell) new ApplicationShell();
        }

        /// <summary>
        /// In UnityBootstrapper class the Run method calls the 
        /// CreateModuleCatalog method and then sets the class's 
        /// ModuleCatalog property using the returned value
        /// </summary>
        protected override void ConfigureModuleCatalog()
        {
            //set logging path
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                              "\\TradeHub Logs\\DataDownloader";
            TraceSourceLogger.Logger.LogDirectory(path);
            base.ConfigureModuleCatalog();
            var moduleCatalog = (ModuleCatalog)ModuleCatalog;
            /*Populate a module catalog from another data
              source by calling the AddModule method or by deriving 
              from ModuleCatalog to create a module catalog with customized behavior.*/
            moduleCatalog.AddModule(typeof(ProviderModule.ProviderModule));
            moduleCatalog.AddModule(typeof(DataModule.DataModule));
        }
    }
}
