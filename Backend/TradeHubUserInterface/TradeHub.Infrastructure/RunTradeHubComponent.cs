using System;
using System.Diagnostics;
using System.Net.Sockets;
using TraceSourceLogger;

namespace TradeHub.UserInterface.Infrastructure
{
    public static class RunTradeHubComponent
    {
        
        private static Process _runningProcess;

        public static void RunComponent(string path)
        {
            try
            {
                if (_runningProcess == null||_runningProcess.HasExited)
                {
                    _runningProcess = Process.Start(path);
                    Logger.Info("Starting the process: " + path, typeof (RunTradeHubComponent).FullName, "RunComponent");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, typeof(RunTradeHubComponent).FullName, "RunComponent");
            }
        }

        /// <summary>
        /// shut down any process running
        /// </summary>
        public static void ShutdownProcess()
        {
            try
            {
                if (_runningProcess != null)
                {
                    Logger.Info("Stopping the process:" + _runningProcess.ProcessName, typeof(RunTradeHubComponent).FullName, "ShutdownProcess");
                    if (_runningProcess.Responding)
                        _runningProcess.CloseMainWindow();
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, typeof(RunTradeHubComponent).FullName, "ShutdownProcess");
            }

        }


    }
}
