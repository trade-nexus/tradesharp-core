using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.PositionEngine.Configuration.Service;
using TradeHub.PositionEngine.ProviderGateway.Service;

namespace TradeHub.PositionEngine.Service
{
    public partial class PEService : ServiceBase
    {
        ApplicationController applicationController;
        public PEService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //set logging path
            Logger.LogDirectory(DirectoryStructure.PE_LOGS_LOCATION);

            applicationController = new ApplicationController(new PositionEngineMqServer("PEMQConfig.xml"), new PositionMessageProcessor());
            applicationController.StartServer();
        }

        protected override void OnStop()
        {
            applicationController.StopServer();
        }
    }
}
