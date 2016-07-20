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
using TradeHub.NotificationEngine.Server.Service;

namespace TradeHub.NotificationEngine.Server.WindowsService
{
    partial class NotificationEngineService : ServiceBase
    {
        private ApplicationController _applicationController;

        public NotificationEngineService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //set logging path
            string path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                              "\\TradeHub Logs\\NotificationEngineServer";
            Logger.LogDirectory(path);
            try
            {
                _applicationController = ContextRegistry.GetContext()["ApplicationController"] as ApplicationController;
                if (_applicationController != null) _applicationController.StartCommunicator();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "NotificationEngineService", "OnStart");
            }
        }

        protected override void OnStop()
        {
            if (_applicationController != null) _applicationController.StopCommunicator();
        }
    }
}
