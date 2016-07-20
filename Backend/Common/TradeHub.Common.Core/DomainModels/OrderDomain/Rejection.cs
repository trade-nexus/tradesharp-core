using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    [Serializable]
    public class Rejection : OrderEvent, ICloneable
    {
        private string _rejectioReason;

        private Rejection() : base(new Security(), "")
        {
            
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public Rejection(Security security, string orderExecutionProvider) : base(security, orderExecutionProvider)
        {
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public Rejection(Security security, string orderExecutionProvider, DateTime dateTime) : base(security, orderExecutionProvider, dateTime)
        {
        }

        /// <summary>
        /// Gets/Sets Order Rejection Reason
        /// </summary>
        public string RejectioReason
        {
            get { return _rejectioReason; }
            set { _rejectioReason = value; }
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        public object Clone()
        {
            return MemberwiseClone();
        }

        public override string ToString()
        {
            return " Rejection :: " +
                   Security +
                   " | Order Execution Provider : " + OrderExecutionProvider +
                   " | Order ID : " + OrderId +
                   " | Rejection Reason : " + RejectioReason;
        }
    }
}
