using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.MarketDataProvider.InteractiveBrokers.ValueObjects
{
    /// <summary>
    /// Contains Properties required for IB Connection
    /// </summary>
    public class ConnectionParameters
    {
        private string _host;
        private int _port;
        private int _clientId;

        #region Properties

        /// <summary>
        /// Gets/Sets the Host used for establishing connection
        /// </summary>
        public string Host
        {
            set { _host = value; }
            get { return _host; }
        }

        /// <summary>
        /// Gets/Sets the Port on which the Market Data is available
        /// </summary>
        public int Port
        {
            set { _port = value; }
            get { return _port; }
        }

        /// <summary>
        /// Gets/Sets Client ID
        /// </summary>
        public int ClientId
        {
            set { _clientId = value; }
            get { return _clientId; }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ConnectionParameters()
        {
            _host = string.Empty;
            _port = default(int);
            _clientId = default(int);
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public ConnectionParameters(string host, int dataPort, int clientId)
        {
            _host = host;
            _port = dataPort;
            _clientId = clientId;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Attributes :: ");
            stringBuilder.Append(" | Host:" + _host);
            stringBuilder.Append(" | Port:" + _port);
            stringBuilder.Append(" | Client ID:" + _clientId);

            return stringBuilder.ToString();
        }
    }
}
