using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.UserInterface.Common.Value_Objects;

namespace TradeHub.UserInterface.Common
{

    public class ServiceParams
    {
        private string _serviceName;
        private ServiceCommand _commandType;

        public ServiceCommand CommandType
        {
            get { return _commandType; }
            set { _commandType = value; }
        }

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }
    }
}
