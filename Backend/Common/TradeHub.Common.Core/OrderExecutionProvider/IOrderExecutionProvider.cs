using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.Common.Core.OrderExecutionProvider
{
    /// <summary>
    /// Interface to be implemented by Order Execution Provider Gateways
    /// </summary>
    public interface IOrderExecutionProvider
    {
        #region Methods

        /// <summary>
        /// Connects/Starts a client
        /// </summary>
        bool Start();

        /// <summary>
        /// Disconnects/Stops a client
        /// </summary>
        bool Stop();

        /// <summary>
        /// Is Order Execution client connected
        /// </summary>
        /// <returns></returns>
        bool IsConnected();

        /// <summary>
        /// Sends Locate message Accepted/Rejected response to Broker
        /// </summary>
        /// <param name="locateResponse">TradeHub LocateResponse Object</param>
        /// <returns></returns>
        bool LocateMessageResponse(LocateResponse locateResponse);

        #endregion

        #region Events
        
        /// <summary>
        /// Fired each time a Logon is arrived
        /// </summary>
        event Action<string> LogonArrived;

        /// <summary>
        /// Fired each time a Logout is arrived
        /// </summary>
        event Action<string> LogoutArrived;

        /// <summary>
        /// Fired each time when order rejection arrives.
        /// </summary>
        event Action<Rejection> OrderRejectionArrived;

        /// <summary>
        /// Fired when a new Locate message is received from broker
        /// </summary>
        event Action<LimitOrder> OnLocateMessage;

        /// <summary>
        /// Fired when a Position message is received from broker
        /// </summary>
        event Action<Position> OnPositionMessage;

        #endregion
    }
}
