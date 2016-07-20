using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.MarketDataEngine.Server.Service;

namespace TradeHub.MarketDataEngine.Server.WindowsService
{
    public partial class MDESerivce : ServiceBase
    {
        ApplicationController applicationController;
        public MDESerivce()
        {
            InitializeComponent();
        }
        Process process;
        protected override void OnStart(string[] args)
        {
            //set logging directory path
            Logger.LogDirectory(DirectoryStructure.MDE_LOGS_LOCATION);
            
             applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
            if (applicationController != null)
            {
                applicationController.StartServer();
                Logger.Info("server started", "Appcontroller", "OnStart");
            }
            else
            {
                Logger.Info("server not started", "Appcontroller", "OnStart");
            }
        }

        protected override void OnStop()
        {
            applicationController.StopServer();
        }
    }
}
