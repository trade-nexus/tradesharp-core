using System;
using System.ServiceProcess;
using TraceSourceLogger;
using TradeHub.UserInterface.Common;
using TradeHub.UserInterface.Common.Interfaces;

namespace TradeHub.UserInterface.Infrastructure
{
    public class ServiceControl:IServiceControl<ServiceParams>
    {
        private Type _type = typeof(ServiceControl);
        private int _timeout = 4000;
        public void StartService(ServiceParams service)
        {
            ServiceController controller = new ServiceController(service.ServiceName);
            try
            {
                if (!ServiceStatus(service.ServiceName).Equals("Running"))
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(_timeout);
                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running, timeout);
                    Logger.Info("Starting service " + service.ServiceName, _type.FullName, "StartService");
                }
                else
                {
                    RestartService(service);
                }
            }
            catch(Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StartService");
            }
        }

        public void StopService(ServiceParams service)
        {
            ServiceController controller = new ServiceController(service.ServiceName);
            try
            {
                if (!ServiceStatus(service.ServiceName).Equals("Stopped"))
                {
                    TimeSpan timeout = TimeSpan.FromMilliseconds(_timeout);
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    Logger.Info("Stoping service " + service.ServiceName, _type.FullName, "StopService");
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "StopService");
            }
        }

        public void RestartService(ServiceParams service)
        {
            ServiceController controller = new ServiceController(service.ServiceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(_timeout);
                if (!ServiceStatus(service.ServiceName).Equals("Stopped"))
                {
                    controller.Stop();
                    controller.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    // count the rest of the timeout
                    int millisec2 = Environment.TickCount;
                    timeout = TimeSpan.FromMilliseconds(_timeout - (millisec2 - millisec1));
                }

                controller.Start();
                controller.WaitForStatus(ServiceControllerStatus.Running, timeout);
                Logger.Info("Restarting service " + service.ServiceName, _type.FullName, "StopService");
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RestartService");
            }
        }

        public string ServiceStatus(string serviceName)
        {
            ServiceController controller = new ServiceController(serviceName);
            try
            {
                return controller.Status.ToString();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RestartService");
            }
            return "Not Responding";
        }
    }
}
