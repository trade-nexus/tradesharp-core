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
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.TradeManager.Server.Service;

namespace TradeHub.TradeManager.Server.WindowsService
{
    public partial class TradeManagerService : ServiceBase
    {
        ApplicationController _applicationController;

        public TradeManagerService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Starts the Windows Service
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            //set logging path
            Logger.LogDirectory(DirectoryStructure.TM_LOGS_LOCATION);

            try
            {
                _applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
                if (_applicationController != null) _applicationController.StartCommunicator();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "TradeManagerService", "OnStart");
            }
        }

        /// <summary>
        /// Stops the Windows Service
        /// </summary>
        protected override void OnStop()
        {
            if (_applicationController != null) _applicationController.StopCommunicator();
        }
    }
}
