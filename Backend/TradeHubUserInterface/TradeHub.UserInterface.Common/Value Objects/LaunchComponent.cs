using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.UserInterface.Common.Value_Objects
{
    public class LaunchComponent
    {
        private TradeHubComponent _component;

        public string Command
        {
            get { return _command; }
            set { _command = value; }
        }

        public TradeHubComponent Component
        {
            get { return _component; }
            set { _component = value; }
        }

        private string _command;

    }
}
