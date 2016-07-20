using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Spring.Context;
using Spring.Context.Support;
using TradeHub.UserInterface.Infrastructure.ProvidersConfigurations;

namespace TradeHub.UserInterface.BootStrap
{
    public class Bootstrapper:UnityBootstrapper
    {
       // private ApplicationController _applicationController;
        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }

        protected override System.Windows.DependencyObject CreateShell()
        {
            ApplicationShell.Shell.ApplicationShell shell=new ApplicationShell.Shell.ApplicationShell();
            return shell;
        }

        protected override void ConfigureModuleCatalog()
        {
           // _applicationController=new ApplicationController();
            //set logging path
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                              "\\TradeHub Logs\\UserInterface";
            TraceSourceLogger.Logger.LogDirectory(path);
            IApplicationContext context = ContextRegistry.GetContext();
            //ProvidersController controller=new ProvidersController();
            base.ConfigureModuleCatalog();
            var moduleCatalog = (ModuleCatalog)ModuleCatalog;
            /*Populate a module catalog from another data
              source by calling the AddModule method or by deriving 
              from ModuleCatalog to create a module catalog with customized behavior.*/
            moduleCatalog.AddModule(typeof(ServicesModule.ServicesModule));
            moduleCatalog.AddModule(typeof (ComponentsModule.ComponentsModule));



        }
    }
}
