using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Constants
{
    public enum ExecutionType
    {
        [Description("Full Fill")]
        Fill,
        [Description("Partial Fill")]
        Partial
    }
}
