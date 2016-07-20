using System;
using System.Text;

namespace TradeHub.MarketDataProvider.Tradier.ValueObject
{
    /// <summary>
    /// Contains Properties required for Tradier Connection
    /// </summary>
    public class Credentials
    {
        private string _apiUrl;
        private string _accessToken;

        #region Properties

        /// <summary>
        /// Gets/Sets the base API Url used for establishing connection
        /// </summary>
        public string ApiUrl
        {
            set { _apiUrl = value; }
            get { return _apiUrl; }
        }

        /// <summary>
        /// Gets/Sets the Access token required for connection
        /// </summary>
        public string AccessToken
        {
            set { _accessToken = value; }
            get { return _accessToken; }
        }

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Credentials()
        {
            _apiUrl = string.Empty;
            _accessToken = string.Empty;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        public Credentials(string apiUrl, string accessToken)
        {
            _apiUrl = apiUrl;
            _accessToken = accessToken;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Attributes :: ");
            stringBuilder.Append(" | API Url:" + _apiUrl);
            stringBuilder.Append(" | Access Token:" +_accessToken);

            return stringBuilder.ToString();
        }
    }
}
