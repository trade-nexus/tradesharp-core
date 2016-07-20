using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.DomainModels
{
    /// <summary>
    /// Strategy status
    /// </summary>
    public enum StrategyStatus
    {
        None,
        Initializing,
        Executing,
        Executed,
        Stopped
    }
}
