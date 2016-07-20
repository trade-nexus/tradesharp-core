using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.UserInterface.Common;

namespace TradeHub.UserInterface.Infrastructure.ProvidersConfigurations
{
    public class ProvidersController
    {
        public ProvidersController()
        {
            Logger.Info("Subscribing Providers events", typeof(ProvidersController).FullName, "ProvidersController");
            EventSystem.Subscribe<ServiceProvider>(ServiceProviderCall);
            EventSystem.Subscribe<ServiceParametersList>(SaveParameterCall);
        }


        private void ServiceProviderCall(ServiceProvider provider)
        {
            Logger.Info("Recieved Command to load parameters", typeof(ProvidersController).FullName, "ServiceProviderCall");
            ProvierParameterReader.LoadParamerters(provider);
        }

        /// <summary>
        /// Save parameters call received
        /// </summary>
        /// <param name="parametersList"></param>
        private void SaveParameterCall(ServiceParametersList parametersList)
        {
            ProvierParameterReader.SaveParameters(parametersList);
        }
    }
}
