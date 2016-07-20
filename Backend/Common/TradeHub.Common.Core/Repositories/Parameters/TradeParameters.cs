using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Repositories.Parameters
{
    /// <summary>
    /// Available Searchable Parameters for Persisted Trades
    /// </summary>
    [Flags]
    public enum TradeParameters
    {
        TradeSide = 0,
        TradeSize = 1,
        Symbol = 2,
        StartTime = 3,
        CompletionTime = 4,
        ExecutionProvider = 5,
        ExecutionId = 10,
        ExecutionSize = 11
    }
}
