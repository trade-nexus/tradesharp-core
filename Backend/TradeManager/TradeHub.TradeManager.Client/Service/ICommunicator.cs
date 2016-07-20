using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.ValueObjects;

namespace TradeHub.TradeManager.Client.Service
{
    /// <summary>
    /// Blueprint for the communicators to give access to Trade Manager Client
    /// </summary>
    public interface ICommunicator
    {
        /// <summary>
        /// Indicates if the communication medium is open or not
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        /// <summary>
        /// Opens necessary connections to start 
        /// </summary>
        void Connect();

        /// <summary>
        /// Closes communication channels
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Forwards executions to Server
        /// </summary>
        void SendExecution(RabbitMqRequestMessage requestMessage);
    }
}
