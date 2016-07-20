using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// Contains Response for the Locate Message
    /// </summary>
    [Serializable]
    public class LocateResponse
    {
        private bool _accepted;
        private string _orderExecutionProvider;
        private string _orderId;
        private string _strategyId;

        private LocateResponse()
        {
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="orderId">OrderID used in the Locate Message</param>
        /// <param name="orderExecutionProvider">Name of the Order Execution Provider which provided the Locate Message</param>
        /// <param name="accepted">Indicate whether Locate Message is accepted or rejected</param>
        public LocateResponse(string orderId, string orderExecutionProvider, bool accepted)
        {
            _accepted = accepted;
            _orderExecutionProvider = orderExecutionProvider;
            _orderId = orderId;
        }

        /// <summary>
        /// OrderID corresponding to Locate Message
        /// </summary>
        public string OrderId
        {
            get { return _orderId; }
            set { _orderId = value; }
        }

        /// <summary>
        /// Name of the Order Execution Provider which provided Locate Message
        /// </summary>
        public string OrderExecutionProvider
        {
            get { return _orderExecutionProvider; }
            set { _orderExecutionProvider = value; }
        }

        /// <summary>
        /// Indicates whether the Locate message is accepted or recjected
        /// </summary>
        public bool Accepted
        {
            get { return _accepted; }
            set { _accepted = value; }
        }

        /// <summary>
        /// Unique Strategy ID to identify the application sending the LocateResponse 
        /// </summary>
        public string StrategyId
        {
            get { return _strategyId; }
            set { _strategyId = value; }
        }

        /// <summary>
        /// Overrider ToString for LocateResponse
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("LocateResponse :: ");
            stringBuilder.Append(" | Order ID: " + _orderId);
            stringBuilder.Append(" | Accepted: " + _accepted);
            stringBuilder.Append(" | Strategy ID: " + _strategyId);
            stringBuilder.Append(" | OEP: " + _orderExecutionProvider);

            return stringBuilder.ToString();
        }
    }
}
