using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.NotificationEngine.Common.ValueObject;

namespace TradeHub.NotificationEngine.Client.Service
{
    /// <summary>
    /// Manages all activities to communicate with the Notification Engine Server
    /// </summary>
    public class NotificationEngineClient
    {
        private Type _type = typeof (NotificationEngineClient);

        /// <summary>
        /// Provides communication medium to interact with server
        /// </summary>
        private readonly ICommunicator _communicator;

        /// <summary>
        /// Indicates if the Client is connected or not
        /// </summary>
        /// <returns></returns>
        public bool IsConnected()
        {
            if (_communicator != null)
            {
                return _communicator.IsConnected();
            }

            return false;
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="communicator">Provides communication medium to interact with server</param>
        public NotificationEngineClient(ICommunicator communicator)
        {
            // Save Instance
            _communicator = communicator;
        }

        /// <summary>
        /// Starts Communicator to open communication medium with clients
        /// </summary>
        public void StartCommunicator()
        {
            // Check for Null Reference
            if (_communicator != null)
            {
                // Check if it not already connected
                if (!_communicator.IsConnected())
                {
                    // Connect Communication Server
                    _communicator.Connect();

                    return;
                }

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Already connected", _type.FullName, "StartCommunicator");
                }
                return;
            }

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Communicator object not initialized", _type.FullName, "StartCommunicator");
            }
        }

        /// <summary>
        /// Stop Communicator to close communication medium
        /// </summary>
        public void StopCommunicator()
        {
            // Check for Null Reference
            if (_communicator != null)
            {
                // Check if the Communicator is currently active
                if (_communicator.IsConnected())
                {
                    // Stop Communication Server
                    _communicator.Disconnect();

                    return;
                }

                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Already disconnected", _type.FullName, "StopCommunicator");
                }
                return;
            }

            if (Logger.IsInfoEnabled)
            {
                Logger.Info("Communicator object not initialized", _type.FullName, "StopCommunicator");
            }
        }

        /// <summary>
        /// Forwards Execution message to Communicator to be sent to Server
        /// </summary>
        /// <param name="notification">Contains Order Notification information to be sent</param>
        public void SendNotification(OrderNotification notification)
        {
            try
            {
                // Forward information to Communicator
                _communicator.SendNotification(notification);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SendNotification");
            }
        }
    }
}
