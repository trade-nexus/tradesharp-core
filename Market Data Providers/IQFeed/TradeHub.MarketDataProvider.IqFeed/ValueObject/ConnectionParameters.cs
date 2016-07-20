using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.MarketDataProvider.IqFeed.ValueObject
{
    public class ConnectionParameters
    {
        private readonly string _loginId;
        private readonly string _password;
        private readonly string _productId;
        private readonly string _productVersion;

        public ConnectionParameters(string loginId, string password, string productId, string productVersion)
        {
            _loginId = loginId;
            _password = password;
            _productId = productId;
            _productVersion = productVersion;
        }

        public string LoginId
        {
            get { return _loginId; }
        }

        public string Password
        {
            get { return _password; }
        }

        public string ProductId
        {
            get { return _productId; }
        }

        public string ProductVersion
        {
            get { return _productVersion; }
        }
    }
}
