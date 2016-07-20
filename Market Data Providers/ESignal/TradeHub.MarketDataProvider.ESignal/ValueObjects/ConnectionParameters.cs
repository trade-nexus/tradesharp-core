using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.MarketDataProvider.ESignal.ValueObjects
{
    /// <summary>
    /// Contains Properties required for ESignal Connection
    /// </summary>
    public class ConnectionParameters
    {
        private string _userName;
        private string _password;

        #region Properties

        /// <summary>
        /// Gets/Sets the Password required to establish connection
        /// </summary>
        public string Password
        {
            set { _password = value; }
            get { return _password; }
        }

        /// <summary>
        /// Gets/Sets the Username used for establishing connection
        /// </summary>
        public string UserName
        {
            set { _userName = value; }
            get { return _userName; }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ConnectionParameters()
        {
            _userName = string.Empty;
            _password = string.Empty;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public ConnectionParameters(string userName, string password)
        {
            _userName = userName;
            _password = password;
        }

        /// <summary>
        /// Overrides ToString method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Attributes :: ");
            stringBuilder.Append(" | Username:" + _userName);
            stringBuilder.Append(" | Password:" + _password);

            return stringBuilder.ToString();
        }
    }
}
