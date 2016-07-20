namespace TradeHub.StrategyRunner.Infrastructure.Entities
{
    /// <summary>
    /// Contains statistics gathered after the Optimization run is complete
    /// </summary>
    public class OptimizationStatistics
    {
        /// <summary>
        /// Contains Execution Statistics
        /// </summary>
        private Statistics _statistics;

        /// <summary>
        /// Contains brief info about the parameters used during optimization run
        /// </summary>
        private string _parametersInfo;

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="statistics">Contains Execution Statistics</param>
        /// <param name="parametersInfo">Contains brief info about the parameters used during optimization run</param>
        public OptimizationStatistics(Statistics statistics, string parametersInfo)
        {
            _statistics = statistics;
            _parametersInfo = parametersInfo;
        }

        /// <summary>
        /// Contains Execution Statistics
        /// </summary>
        public Statistics Statistics
        {
            get { return _statistics; }
            set { _statistics = value; }
        }

        /// <summary>
        /// Contains brief info about the parameters used during optimization run
        /// </summary>
        public string ParametersInfo
        {
            get { return _parametersInfo; }
            set { _parametersInfo = value; }
        }
    }
}
