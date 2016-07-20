using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.UserInterface.Infrastructure.ProvidersConfigurations
{
    public class ServiceParametersList
    {
        private ServiceProvider _serviceProvider;
        private List<Parameters> _parametersList;

        public List<Parameters> ParametersList
        {
            get { return _parametersList; }
        }

        public ServiceProvider ServiceProvider
        {
            get { return _serviceProvider; }
        }


        public ServiceParametersList(ServiceProvider serviceProvider,List<Parameters> paramtersList)
        {
            _serviceProvider = serviceProvider;
            _parametersList = paramtersList;
        }
    }
}
