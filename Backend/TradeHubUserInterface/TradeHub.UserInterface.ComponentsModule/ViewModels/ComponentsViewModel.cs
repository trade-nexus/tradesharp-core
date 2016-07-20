using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TraceSourceLogger;
using TradeHub.UserInterface.Common;
using TradeHub.UserInterface.Common.Value_Objects;
using TradeHub.UserInterface.ComponentsModule.Commands;

namespace TradeHub.UserInterface.ComponentsModule.ViewModels
{
    public class ComponentsViewModel
    {
        private Type _type = typeof (ComponentsViewModel);
        public ICommand OpenStrategyRunnerCommand { get; set; }
        public ICommand OpenMddCommand { get; set; }
        public ICommand OpenClerkCommand { get; set; }

        public ComponentsViewModel()
        {
            OpenStrategyRunnerCommand=new OpenStrategyRunnerCommand(this);
            OpenMddCommand=new OpenDataDownloaderCommand(this);
            OpenClerkCommand=new OpenClerkCommand(this);
            
        }

        /// <summary>
        /// Run the Strategy Runner
        /// </summary>
        public void OpenStrategyRunner()
        {
            LaunchComponent component=new LaunchComponent(){Command = "Run",Component = TradeHubComponent.StrategyRunner};
            EventSystem.Publish<LaunchComponent>(component);
            Logger.Info("Event Published to run StrategyRunner", _type.FullName, "OpenStrategyRunner");
        }

        /// <summary>
        /// Run Market Data Downloader
        /// </summary>
        public void OpenMdd()
        {
            
        }

        /// <summary>
        /// Open Clerk
        /// </summary>
        public void OpenClerk()
        {
            LaunchComponent component = new LaunchComponent() { Command = "Run", Component = TradeHubComponent.Clerk };
            EventSystem.Publish<LaunchComponent>(component);
            Logger.Info("Event Published to run Clerk", _type.FullName, "OpenClerk");
            
        }
    }
}
