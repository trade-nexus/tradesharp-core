using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Fix.Constants
{
    /// <summary>
    /// Order status
    /// </summary>
    public static class FixOrderStatus
    {
        /// <summary>
        /// Pending new. Order has been received by brokers system but not yet accepted for execution. An Execution Report (8) 
        /// message with this status will only be sent in response to a Status Request (H) message. 
        /// </summary>
        public const char PendingNew = 'A';
        /// <summary>
        /// New. Outstanding order with no executions.
        /// </summary>
        public const char New = '0';
        /// <summary>
        /// Accepted for bidding. Order has been received and is being evaluated for pricing. It is anticipated that this status will 
        /// only be used with the "Disclosed" BidType (394) List Order Trading model
        /// </summary>
        public const char AcceptedForBidding = 'D';
        /// <summary>
        /// Partially filled. Outstanding order with executions and remaining quantity.
        /// </summary>
        public const char PartiallyFilled = '1';
        /// <summary>
        /// Filled. Order completely filled, no remaining quantity.
        /// </summary>
        public const char Filled = '2';
        /// <summary>
        /// Rejected. Order has been rejected by broker. NOTE: An order can be rejected subsequent to order acknowledgment, i.e. an order can pass from New to Rejected status. 
        /// </summary>
        public const char Rejected = '8';
        /// <summary>
        /// Expired. Order has been canceled in broker\s system due to time in force instructions.
        /// </summary>
        public const char Expired = 'C';
        /// <summary>
        /// Cancelled. Canceled order with or without executions.
        /// </summary>
        public const char Canceled = '4';
        /// <summary>
        /// Replaced. Replaced order with or without executions.
        /// </summary>
        public const char Replaced = '5';
        /// <summary>
        /// Pending cancel (e.g. result of Order Cancel Request (F) ). Order with an Order Cancel Request pending, used to confirm 
        /// receipt of an Order Cancel Request (F) . DOES NOT INDICATE THAT THE ORDER HAS BEEN CANCELED. 
        /// </summary>
        public const char PendingCancel = '6';
        /// <summary>
        /// Pending Replace (e.g. result of Order Cancel/Replace Request (G)). Order with an Order Cancel/Replace Request pending, used to confirm 
        /// receipt of an Order Cancel/Replace Request (G) . DOES NOT INDICATE THAT THE ORDER HAS BEEN REPLACED. 
        /// </summary>
        public const char PendingReplace = 'E';
    }
}
