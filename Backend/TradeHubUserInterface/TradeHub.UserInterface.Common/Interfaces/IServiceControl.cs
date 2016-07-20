using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.UserInterface.Common.Interfaces
{
    public interface IServiceControl<T>
    {
        /// <summary>
        /// Method to start the service
        /// </summary>
        /// <param name="service"></param>
        void StartService(T service);

        /// <summary>
        /// Method to stop the service
        /// </summary>
        /// <param name="service"></param>
        void StopService(T service);

        /// <summary>
        /// Method to restart the service
        /// </summary>
        /// <param name="service"></param>
        void RestartService(T service);

        /// <summary>
        /// Check the service status
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        string ServiceStatus(string serviceName);
    }
}
