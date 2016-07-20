using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Repositories.Parameters
{
    [Flags]
    public enum FillParameters
    {
        ExecutionSize = 0,
        ExecutionPrice = 1,
        ExecutionType = 2,
        ExecutionSide = 3,
        OrderId = 4,
        OrderExecutionProvider = 20,
        Symbol = 21,
        StartegyId = 22
    }
}
