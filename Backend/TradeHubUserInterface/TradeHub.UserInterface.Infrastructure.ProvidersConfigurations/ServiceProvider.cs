using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.UserInterface.Infrastructure.ProvidersConfigurations
{
    /// <summary>
    /// Value object for passing service and provider name
    /// </summary>
    public class ServiceProvider
    {
        private string _serviceName;
        private string _providerName;

        public string ProviderName
        {
            get { return _providerName; }
        }

        public string ServiceName
        {
            get { return _serviceName; }
        }

        public ServiceProvider(string serviceName,string providerName)
        {
            _serviceName = serviceName;
            _providerName = providerName;
            
        }
    }
}
