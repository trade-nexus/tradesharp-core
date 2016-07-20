using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.MarketDataProvider.Fxcm.ValueObject
{
    public class ConnectionParameters
    {
        private readonly string _loginId;
        private readonly string _password;
        private readonly string _account;
        private readonly string _connection;
        private readonly string _url;
        private string _sessionId;
        private string _pin;

        /// <summary>
        /// Argument constructor
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="password"></param>
        /// <param name="account"></param>
        /// <param name="connection"></param>
        /// <param name="url"></param>
        public ConnectionParameters(string loginId, string password, string account, string connection, string url)
        {
            _loginId = loginId;
            _password = password;
            _account = account;
            _connection = connection;
            _url = url;
        }

        public string LoginId
        {
            get { return _loginId; }
        }

        public string Password
        {
            get { return _password; }
        }

        public string Account
        {
            get { return _account; }
        }

        public string Connection
        {
            get { return _connection; }
        }

        public string SessionId
        {
            get { return _sessionId; }
            set { _sessionId = value; }
        }

        public string Pin
        {
            get { return _pin; }
            set { _pin = value; }
        }

        public string Url
        {
            get { return _url; }
        }
    }
}
