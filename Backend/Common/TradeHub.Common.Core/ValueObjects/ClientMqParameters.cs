using System.Text;

namespace TradeHub.Common.Core.ValueObjects
{
    /// <summary>
    /// Contains Client MQ info 
    /// </summary>
    public class ClientMqParameters
    {
        private string _appId = string.Empty;
        private string _replyTo = string.Empty;
        private string _consumerTag = string.Empty;
        private ulong _deliverTag = default(ulong);
        private string _exchangeName = string.Empty;
        private string _routingKey = string.Empty;

        /// <summary>
        /// Gets/Sets App ID
        /// </summary>
        public string AppId
        {
            get { return _appId; }
            set { _appId = value; }
        }

        /// <summary>
        /// Gets/Sets Consumer Tag
        /// </summary>
        public string ConsumerTag
        {
            get { return _consumerTag; }
            set { _consumerTag = value; }
        }

        /// <summary>
        /// Gets/Sets Deliver Tag
        /// </summary>
        public ulong DeliverTag
        {
            get { return _deliverTag; }
            set { _deliverTag = value; }
        }

        /// <summary>
        /// Gets/Sets ExchangeName
        /// </summary>
        public string ExchangeName
        {
            get { return _exchangeName; }
            set { _exchangeName = value; }
        }

        /// <summary>
        /// Gets/Sets RoutingKey
        /// </summary>
        public string RoutingKey
        {
            get { return _routingKey; }
            set { _routingKey = value; }
        }

        /// <summary>
        /// Name of the Queue on which to Reply
        /// </summary>
        public string ReplyTo
        {
            get { return _replyTo; }
            set { _replyTo = value; }
        }

        /// <summary>
        /// Overrides ToString Method to provide Client MQ Parameters Info
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("ClientMqParameters :: ");
            stringBuilder.Append(" | App ID: " + AppId);
            stringBuilder.Append(" | Reply To: " + ReplyTo);
            stringBuilder.Append(" | Consumer Tag: " + ConsumerTag);
            stringBuilder.Append(" | Deliver Tag: " + DeliverTag);
            stringBuilder.Append(" | Exchange Name: " + ExchangeName);
            stringBuilder.Append(" | Routing Key: " + RoutingKey);
            return stringBuilder.ToString();
        }
    }
}
