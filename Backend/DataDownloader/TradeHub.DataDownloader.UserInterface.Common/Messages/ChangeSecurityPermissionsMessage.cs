using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.DataDownloader.Common.ConcreteImplementation;

namespace TradeHub.DataDownloader.UserInterface.Common.Messages
{
    public class ChangeSecurityPermissionsMessage
    {
        public SecurityPermissions Permissions { get; set; }
    }
}
