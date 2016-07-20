using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.TradeManager.CommunicationManager.Service
{
    /// <summary>
    /// Blueprint for the communicators to give access to Trade Manager Server
    /// </summary>
    public interface ICommunicator
    {
        /// <summary>
        /// Raised when new Execution Message is received
        /// </summary>
        event Action<Execution> NewExecutionReceivedEvent;

        /// <summary>
        /// Checks if the medium is available for communication or not
        /// </summary>
        bool IsConnected();

        /// <summary>
        /// Connect necessary services to start communication
        /// </summary>
        void Connect();

        /// <summary>
        /// Disconnect necessary services to stop communication
        /// </summary>
        void Disconnect();
    }
}
