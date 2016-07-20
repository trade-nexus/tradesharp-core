using System;
using System.Text;

namespace TradeHub.MarketDataProvider.Redi.ValueObject
{
    /// <summary>
    /// Contains Properties required for REDI Connection
    /// </summary>
    public class Credentials
    {
        private string _username;
        private string _password;
        private string _ipAddress;
        private string _port;

        #region Properties

        /// <summary>
        /// Gets/Sets Username for connection
        /// </summary>
        public string Username
        {
            set { _username = value; }
            get { return _username; }
        }

        /// <summary>
        /// Gets/Sets Password for connection
        /// </summary>
        public string Password
        {
            set { _password = value; }
            get { return _password; }
        }

        /// <summary>
        /// IP Address to  be used on which to send request
        /// </summary>
        public string IpAddress
        {
            get { return _ipAddress; }
            set { _ipAddress = value; }
        }

        /// <summary>
        /// Port to be used along with the IP
        /// </summary>
        public string Port
        {
            get { return _port; }
            set { _port = value; }
        }

        #endregion

        public Credentials()
        {
            
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public Credentials(string username, string password, string ipAddress, string port)
        {
            _username = username;
            _password = password;
            _ipAddress = ipAddress;
            _port = port;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Attributes :: ");
            stringBuilder.Append(" | Username:" + _username);
            stringBuilder.Append(" | Ip Address:" + _ipAddress);
            stringBuilder.Append(" | Port:" + _port);

            return stringBuilder.ToString();
        }
    }
}
