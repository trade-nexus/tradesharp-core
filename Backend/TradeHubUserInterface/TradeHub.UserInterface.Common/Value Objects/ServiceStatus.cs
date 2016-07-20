using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.UserInterface.Common.Value_Objects
{
    /// <summary>
    /// Carry service status information
    /// </summary>
    public class ServiceStatus
    {
        private string _serviceName;
        private string _status;

        public string Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public string ServiceName
        {
            get { return _serviceName; }
            set { _serviceName = value; }
        }
    }
}
