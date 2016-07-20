using System;

namespace TradeHub.Common.Core.Repositories.Parameters
{
    /// <summary>
    /// Available Searchable Parameters for Persisted Orders
    /// </summary>
    [Flags]
    public enum OrderParameters
    {
        OrderId = 0,
        OrderSide = 1,
        OrderSize = 2,
        TriggerPrice = 3,
        LimitPrice = 4,
        Symbol = 5,
        OrderStatus = 6,
        OrderDateTime = 7,
        StartOrderDateTime = 8,
        EndOrderDateTime = 9,
        StrategyId = 10,
        OrderExecutionProvider = 11,
        discriminator=12,
        ExecutionId = 20,
        ExecutionPrice = 21,
        ExecutionSize = 22
    }
}
