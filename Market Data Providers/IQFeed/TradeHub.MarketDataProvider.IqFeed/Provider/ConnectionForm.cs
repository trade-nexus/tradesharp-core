using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using TraceSourceLogger;

namespace TradeHub.MarketDataProvider.IqFeed.Provider
{
    internal class ConnectionForm
    {
        private Type _type = typeof(ConnectionForm);

        /// <summary>
        /// Indicates if the Connection Form is working
        /// </summary>
        private bool _connected;

        /// <summary>
        /// Login ID
        /// </summary>
        private string _loginId;

        /// <summary>
        /// Password
        /// </summary>
        private string _password;

        /// <summary>
        /// Product ID provided by IQ for the give Login ID
        /// </summary>
        private string _productId;

        /// <summary>
        /// Product version of the IQ Feed Connector installed
        /// </summary>
        private string _productVersion;

        /// <summary>
        /// Class level logger used in the calling class
        /// </summary>
        private readonly AsyncClassLogger _logger;

        // Global variables for socket communications
        AsyncCallback _adminCallback;
        Socket _adminSocket;
        byte[] _adminSocketBuffer = new byte[8096];
        string _adminIncompleteRecord = "";
        bool _adminNeedBeginReceive = true;
        private bool _registered = false;

        // Holds reference to the process in which IQConnect.exe is running
        private Process _iqFeedConnectorProcess;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="logger">AsyncClassLogger object used in the calling class</param>
        public ConnectionForm(AsyncClassLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Indicates if the Connection Form is working
        /// </summary>
        public bool Connected
        {
            get { return _connected; }
        }

        /// <summary>
        /// Launches the IQ Feed Connector form and starts connection
        /// </summary>
        /// <param name="loginId">Login ID</param>
        /// <param name="password">Password</param>
        /// <param name="productId">Product ID provided by IQ for the give Login ID</param>
        /// <param name="productVersion">Prouct version of the IQ Feed Connector installed</param>
        /// <returns></returns>
        public bool Connect(string loginId, string password, string productId, string productVersion)
        {
            try
            {
                _loginId = loginId;
                _password = password;
                _productId = productId;
                _productVersion = productVersion;

                if (LaunchForm())
                {
                    // Opens connections with the Form
                    CreateConnection();

                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "LaunchForm");
                return false;
            }
        }

        /// <summary>
        /// Opens IQ Feed Connector application
        /// </summary>
        /// <returns></returns>
        private bool LaunchForm()
        {
            try
            {
                // The below code builds the command line parameters user chose.
                string arguments = "";
                if (_productId.Length > 0)
                {
                    arguments += "-product " + _productId + " ";
                }
                if (_productVersion.Length > 0)
                {
                    arguments += "-version " + _productVersion + " ";
                }
                if (_loginId.Length > 0)
                {
                    arguments += "-login " + _loginId + " ";
                }
                if (_password.Length > 0)
                {
                    arguments += "-password " + _password + " ";
                }

                arguments += "-savelogininfo ";

                // arguments += "-autoconnect";
                arguments = arguments.TrimEnd(' ');

                // Will launch IQConnect.exe
                _iqFeedConnectorProcess = System.Diagnostics.Process.Start("IQConnect.exe", arguments);

                // Set our registered flag for later validation
                _registered = true;

                return true;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName,"LaunchForm");
                return false;
            }
        }

        /// <summary>
        /// Creates connection with the IQ Feed Connection form application.
        /// </summary>
        private void CreateConnection()
        {
            // Create the socket for Admin communication
            _adminSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Set IP Address
            IPAddress ipLocalhost = IPAddress.Parse("127.0.0.1");

            // Set Port
            int iPort = GetIQFeedPort("Admin");

            // Create End Point
            IPEndPoint ipendLocalhost = new IPEndPoint(ipLocalhost, iPort);
            try
            {
                // Connect
                _adminSocket.Connect(ipendLocalhost);

                // Call  WaitForData function to notify the socket that we are ready to receive callbacks when new data arrives
                WaitForData("Admin");
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName, "CreateConnection");
            }
        }

        /// <summary>
        /// We call this to notify the .NET Async socket to start listening for data to come in.  It must be called each time after we receive data
        /// </summary>
        private void WaitForData(string socketName)
        {
            if (socketName.Equals("Admin"))
            {
                // make sure we have a callback created
                if (_adminCallback == null)
                {
                    _adminCallback = new AsyncCallback(OnReceive);
                }
                // send the notification to the socket
                if (_adminNeedBeginReceive)
                {
                    _adminNeedBeginReceive = false;
                    _adminSocket.BeginReceive(_adminSocketBuffer, 0, _adminSocketBuffer.Length, SocketFlags.None, _adminCallback, socketName);
                }
            }
        }

        /// <summary>
        /// This is our callback that gets called by the .NET socket class when new data arrives on the socket
        /// </summary>
        /// <param name="asyncResult"></param>
        private void OnReceive(IAsyncResult asyncResult)
        {
            try
            {
                // First verify we received data from the correct socket.
                if (asyncResult.AsyncState.ToString().Equals("Admin"))
                {
                    _connected = true;

                    // Read data from the socket
                    int iReceivedBytes = 0;
                    iReceivedBytes = _adminSocket.EndReceive(asyncResult);

                    // Set our flag back to true so we can call begin receive again
                    _adminNeedBeginReceive = true;

                    // Convert to a string for ease of use.
                    string data = Encoding.ASCII.GetString(_adminSocketBuffer, 0, iReceivedBytes);

                    // We need to save off any incomplete messages while processing the data and add them to the beginning of the data next time.
                    data = _adminIncompleteRecord + data;

                    // clear our incomplete record string so it doesn't get processed next time too.
                    _adminIncompleteRecord = "";

                    while (data.Length > 0)
                    {
                        int iNewLinePos = -1;
                        iNewLinePos = data.IndexOf("\n");
                        string sLine;
                        if (iNewLinePos == -1)
                        {
                            // we have an incomplete record.  Save it off for the next call of OnRecieve.
                            _adminIncompleteRecord = data;
                            data = "";
                        }
                        else
                        {
                            // we have a complate record.  Process it.
                            sLine = data.Substring(0, iNewLinePos);
                            if (sLine.StartsWith("S,STATS,"))
                            {
                                // check if we registered using the command line parameters
                                if (!_registered)
                                {
                                    // we need to register the feed, send off the S,REGISTER CLIENT APP command
                                    string sCommand = "S,REGISTER CLIENT APP,";
                                    sCommand += _productId;
                                    sCommand += ",";
                                    sCommand += _productVersion;
                                    sCommand += "\r\n";

                                    // and we send it to the feed via the Admin socket
                                    byte[] szCommand = new byte[sCommand.Length];
                                    szCommand = Encoding.ASCII.GetBytes(sCommand);
                                    int iBytesToSend = szCommand.Length;
                                    _adminSocket.Send(szCommand, iBytesToSend, SocketFlags.None);
                                    // set our flag so that we don't send the command again with the next stats message.
                                    _registered = true;
                                }
                            }
                            else if (sLine.StartsWith("S,REGISTER CLIENT APP COMPLETED"))
                            {
                                // our S,REGISTER CLIENT APP command completed. 

                                // Send our loginID
                                string command = "";
                                command += "S,SET LOGINID,";
                                command += _loginId;
                                command += "\r\n";
                                // Password
                                command += "S,SET PASSWORD,";
                                command += _password;
                                command += "\r\n";
                                // save login info (based on user input or previous settings)
                                command += "S,SET SAVE LOGIN INFO,";
                                command += "On\r\n";
                                // autoconnect (based on user input or previous settings)
                                command += "S,SET AUTOCONNECT,";
                                command += "On\r\n";
                                // and issue a connect command.  If Autoconnect is NOT set, 
                                // this will prompt the user to click connect.
                                command += "S,CONNECT\r\n";

                                // we send it to the feed via the Admin socket
                                byte[] commandBytes = new byte[command.Length];
                                commandBytes = Encoding.ASCII.GetBytes(command);
                                int iBytesToSend = commandBytes.Length;
                                _adminSocket.Send(commandBytes, iBytesToSend, SocketFlags.None);
                                _registered = true;
                            }
                            data = data.Substring(sLine.Length + 1);
                        }
                    }

                    // call wait for data to notify the socket that we are ready to receive another callback
                    WaitForData("Admin");
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, _type.FullName,"OnReceive");
                _connected = false;
            }
        }

        /// <summary>
        /// Gets local IQFeed socket ports from the registry
        /// </summary>
        /// <param name="sPort"></param>
        /// <returns></returns>
        private int GetIQFeedPort(string sPort)
        {
            int port = 0;
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\DTN\\IQFeed\\Startup");
            if (key != null)
            {
                string sData = "";
                switch (sPort)
                {
                    case "Level1":
                        // the default port for Level 1 data is 5009.
                        sData = key.GetValue("Level1Port", "5009").ToString();
                        break;
                    case "Lookup":
                        // the default port for Lookup data is 9100.
                        sData = key.GetValue("LookupPort", "9100").ToString();
                        break;
                    case "Level2":
                        // the default port for Level 2 data is 9200.
                        sData = key.GetValue("Level2Port", "9200").ToString();
                        break;
                    case "Admin":
                        // the default port for Admin data is 9300.
                        sData = key.GetValue("AdminPort", "9300").ToString();
                        break;
                    case "Derivative":
                        // the default port for derivative data is 9400
                        sData = key.GetValue("DerivativePort", "9400").ToString();
                        break;
                }
                port = Convert.ToInt32(sData);
            }
            return port;
        }

        /// <summary>
        /// Stops all necessary connections
        /// </summary>
        public void Stop()
        {
            _adminSocket.Close();

            // Will close IQConnect.exe
            _iqFeedConnectorProcess.Close();
        }
    }
}
