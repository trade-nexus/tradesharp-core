using System;
using System.Collections.Generic;

namespace TradeHub.Common.Core.Constants
{
    /// <summary>
    /// static class to represent orderstatus
    /// </summary>
    public static class OrderStatus
    {
        // ReSharper disable InconsistentNaming
        public const string OPEN = "OPEN";
        public const string SUBMITTED = "SUBMITTED";
        public const string EXECUTED = "EXECUTED";
        public const string PARTIALLY_EXECUTED = "PARTIALLY_EXECUTED";
        public const string CANCELLED = "CANCELLED";
        public const string REJECTED = "REJECTED";
        // ReSharper restore InconsistentNaming

    }
}
