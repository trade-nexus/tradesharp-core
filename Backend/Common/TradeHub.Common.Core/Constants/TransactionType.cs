using System;
using System.Collections.Generic;

namespace TradeHub.Common.Core.Constants
{
    /// <summary>
    /// Immutable class to represent transaction type.
    /// </summary>
    public static class TransactionType
    {
        // ReSharper disable InconsistentNaming
        public const string BUY = "BUY";
        public const string SELL = "SELL";
        public const string TRANSFER = "TRANSFER";
        public const string CREDIT = "CREDIT";
        public const string DEBIT = "DEBIT";
        // ReSharper restore InconsistentNaming
    }
}
