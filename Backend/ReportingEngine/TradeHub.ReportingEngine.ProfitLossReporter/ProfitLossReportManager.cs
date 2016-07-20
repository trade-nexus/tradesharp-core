using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.Core.Repositories.Parameters;
using TradeHub.Infrastructure.Nhibernate.Repositories;

namespace TradeHub.ReportingEngine.ProfitLossReporter
{
    /// <summary>
    /// Responsible for generating all Profit and Loss related Reports
    /// </summary>
    public class ProfitLossReportManager
    {
        private Type _type = typeof (ProfitLossReportManager);

        /// <summary>
        /// Provides Access to Database
        /// </summary>
        private readonly ITradeRepository _tradeRepository;
        
        #region Events

        public event Action<ProfitLossStats> DataReceived;

        #endregion

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="tradeRepository">Provides access to Database</param>
        public ProfitLossReportManager(ITradeRepository tradeRepository)
        {
            // Save Instance
            _tradeRepository = tradeRepository;
        }

        /// <summary>
        /// Requests Infrastructure for specified information
        /// </summary>
        /// <param name="arguments">Report arguments</param>
        public void RequestReport(Dictionary<TradeParameters, string> arguments)
        {
            try
            {
                //Request required information from DB
                IList<Trade> result = _tradeRepository.Filter(arguments);

                // Check if the received result value is not NULL
                if (result != null)
                {
                    // Create Profit and Loss object
                    ProfitLossStats profitLoss = new ProfitLossStats(result);

                    // Raise Event
                    DataReceived(profitLoss);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RequestReport");
            }
        }
    }
}
