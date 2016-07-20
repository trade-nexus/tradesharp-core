using System;

namespace TradeHub.Common.Core.Constants
{
    /// <summary>
    /// static class to represent time in force (tif)
    /// </summary>
    public static class OrderTif
    {
        // ReSharper disable InconsistentNaming
        public const string DAY = "DAY";
        public const string GTC = "GTC";
        public const string GTD = "GTD";
        public const string GTX = "GTX";
        public const string IOC = "IOC";
        public const string OPG = "OPG";
        public const string FOK = "FOK";
        public const string NONE = "NONE";
        // ReSharper restore InconsistentNaming
    }
}
