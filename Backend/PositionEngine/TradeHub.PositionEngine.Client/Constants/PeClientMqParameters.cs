using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.PositionEngine.Client.Constants
{
    public class PeClientMqParameters
    {
        public static string ConnectionString = "ConnectionString";
        public static string Exchange = "Exchange";
        public static string InquiryResponseQueue = "InquiryResponseQueue";
        public static string InquiryResponseRoutingKey = "InquiryResponseRoutingKey";
        public static string PositionMessageQueue = "PositionMessageQueue";
        public static string PositionMessageRoutingKey = "PositionMessageRoutingKey";

    }
}
