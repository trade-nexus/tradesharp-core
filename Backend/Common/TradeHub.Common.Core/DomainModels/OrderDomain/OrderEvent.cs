using System;

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// Represents any type of Order Execution related to a particular Security.
    /// </summary>
    [Serializable]
    public class OrderEvent
    {
        private DateTime _dateTime;
        private Security _security;
        private string _orderId;
        private string _orderExecutionProvider;

        /// <summary>
        /// Gets/Sets the Security
        /// </summary>
        public Security Security
        {
            get { return _security; }
            set { _security = value; }
        }

        /// <summary>
        /// Gets/Sets the Data Event Time
        /// </summary>
        public DateTime DateTime
        {
            get { return _dateTime; }
            set { _dateTime = value; }
        }

        /// <summary>
        /// Gets/Sets name of the Market Data Provider
        /// </summary>
        public string OrderExecutionProvider
        {
            get { return _orderExecutionProvider; }
            set { _orderExecutionProvider = value; }
        }

        /// <summary>
        /// Gets/Sets Order ID (Must be Unique)
        /// </summary>
        public string OrderId
        {
            get { return _orderId; }
            set { _orderId = value; }
        }

        public OrderEvent()
        {
            
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public OrderEvent(Security security, string orderExecutionProvider)
        {
            _security = security;
            _dateTime = DateTime.Now;
            _orderExecutionProvider = orderExecutionProvider;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public OrderEvent(Security security, string orderExecutionProvider, DateTime dateTime)
        {
            _security = security;
            _orderExecutionProvider = orderExecutionProvider;
            _dateTime = dateTime;
        }
    }
}
