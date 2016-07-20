using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.Repositories;
using TradeHub.Common.Core.Repositories.Parameters;
using TradeHub.Infrastructure.Nhibernate.Repositories;

namespace TradeHub.ReportingEngine.OrderReporter
{
    /// <summary>
    /// Responsible for generating all order related reports
    /// </summary>
    public class OrderReportManager
    {
        private Type _type = typeof (OrderReportManager);

        /// <summary>
        /// Provides Access to DataBase
        /// </summary>
        private IOrderRepository _orderRespository;

        #region Events

        public event Action<IList<object[]>> DataReceived; 

        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="orderRespository"></param>
        public OrderReportManager(IOrderRepository orderRespository)
        {
            _orderRespository = orderRespository;
        }

        /// <summary>
        /// Requests infrastructure for specified information
        /// </summary>
        /// <param name="arguments"></param>
        public void RequestReport(Dictionary<OrderParameters, string> arguments)
        {
            try
            {
                // Request required information from DB
                IList<object[]> result = _orderRespository.Find(arguments);

                // Raise Event to notify Listeners
                if (result != null)
                {
                    DataReceived(result);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "RequestReport");
            }
        }
    }
}
