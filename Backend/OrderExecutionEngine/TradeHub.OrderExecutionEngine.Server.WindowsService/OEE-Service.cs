using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Spring.Context.Support;
using TradeHub.OrderExecutionEngine.Server.Service;
using System.Xml;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;

namespace TradeHub.OrderExecutionEngine.Server.WindowsService
{
    public partial class OEEService : ServiceBase
    {
        ApplicationController applicationController;
        public OEEService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //set logging path
            Logger.LogDirectory(DirectoryStructure.OEE_LOGS_LOCATION);

            //set path to current directory
            System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
            try
            {
                 applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
                if (applicationController != null) applicationController.StartServer();
                
            }
            catch (Exception exe)
            {
                Logger.Error(exe,"OEE-Service","OnStart");
 
            }
        }

        protected override void OnStop()
        {
            applicationController.StopServer();
        }
    }
}
