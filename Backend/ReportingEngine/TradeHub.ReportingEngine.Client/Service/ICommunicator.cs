using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories.Parameters;

namespace TradeHub.ReportingEngine.Client.Service
{
    /// <summary>
    /// Blueprint for the communicators to give functionality to Reproting Engine Client
    /// </summary>
    public interface ICommunicator
    {
        /// <summary>
        /// Raised when Order Report is received from Reporting Engine
        /// </summary>
        event Action<IList<object[]>> OrderReportReceivedEvent; 

        /// <summary>
        /// Raised when Profit Loss Report is received from Reporting Engine
        /// </summary>
        event Action<ProfitLossStats> ProfitLossReportReceivedEvent; 

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
        /// Send Order Report request to Reporting Engine
        /// </summary>
        /// <param name="parameters">Search parameters to be used for report</param>
        void RequestOrderReport(Dictionary<OrderParameters, string> parameters);

        /// <summary>
        /// Send Profit Loss Report request to Reporting Engine
        /// </summary>
        /// <param name="parameters">Search parameters to be used for report</param>
        void RequestProfitLossReport(Dictionary<TradeParameters, string> parameters);
    }
}
