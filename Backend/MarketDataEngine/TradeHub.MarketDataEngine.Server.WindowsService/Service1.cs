using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.MarketDataEngine.Server.WindowsService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }
        Process process;
        protected override void OnStart(string[] args)
        {
            process = Process.Start(@"C:\Program Files (x86)\TradeHub\Mde\TradeHub.MarketDataEngine.Server.exe");
        }

        protected override void OnStop()
        {           
           process.Kill();
        }
    }
}
