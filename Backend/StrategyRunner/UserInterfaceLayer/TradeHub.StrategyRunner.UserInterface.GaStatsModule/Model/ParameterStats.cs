using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.UserInterface.GaStatsModule.Model
{
    /// <summary>
    /// Contains info for the optimized parameter
    /// </summary>
    public class ParameterStats
    {
        /// <summary>
        /// Given parameter discription
        /// </summary>
        private readonly string _discription;

        /// <summary>
        /// Optimized parameter value
        /// </summary>
        private readonly double _parameterValue;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="discription">Given parameter discription</param>
        /// <param name="parameterValue">Optimized parameter value</param>
        public ParameterStats(string discription, double parameterValue)
        {
            _discription = discription;
            _parameterValue = parameterValue;
        }

        /// <summary>
        /// Given parameter discription
        /// </summary>
        public string Discription
        {
            get { return _discription; }
        }

        /// <summary>
        /// Optimized parameter value
        /// </summary>
        public double ParameterValue
        {
            get { return _parameterValue; }
        }
    }
}
