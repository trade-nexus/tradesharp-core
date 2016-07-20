using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.Constants
{
    /// <summary>
    /// Contains Tags supported by Inquiry Message
    /// </summary>
    public static class InquiryTags
    {
        // ReSharper disable InconsistentNaming
        public const string AppID = "AppID";
        public const string DisconnectClient = "DisconnectClient";
        public const string MarketDataProviderInfo = "MDPInfo";
        public const string OrderExecutionProviderInfo = "OEPInfo";
        // ReSharper restore InconsistentNaming
    }
}