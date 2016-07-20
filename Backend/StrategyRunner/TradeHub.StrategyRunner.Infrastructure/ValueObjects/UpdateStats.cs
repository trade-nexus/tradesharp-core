using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    public class UpdateStats
    {
        /// <summary>
        /// Contains the Execution Info for the order executed
        /// </summary>
        private Execution _execution;

        /// <summary>
        /// Contains the Execution Info for the order executed
        /// </summary>
        public Execution Execution
        {
            get { return _execution; }
            set { _execution = value; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="execution">Contains the Execution Info for the order executed</param>
        public UpdateStats(Execution execution)
        {
            _execution = execution;
        }
    }
}
