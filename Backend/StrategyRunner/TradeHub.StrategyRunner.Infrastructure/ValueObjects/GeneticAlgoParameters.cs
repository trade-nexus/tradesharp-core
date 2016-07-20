namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains parameters to be used for Genetic Algorithm Optimization
    /// </summary>
    public class GeneticAlgoParameters
    {
        /// <summary>
        /// Index of the parameter in discussion
        /// </summary>
        private int _index;

        /// <summary>
        /// Description of the Parameter
        /// </summary>
        private string _description;

        /// <summary>
        /// Start point of parameter range
        /// </summary>
        private double _startValue;

        /// <summary>
        /// End point of parameter range
        /// </summary>
        private double _endValue;

        /// <summary>
        /// Increment factor to be used when moving from StartValue to EndValue
        /// </summary>
        //private double _incrementFactor;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public GeneticAlgoParameters()
        {
            _index = default(int);
            _description = string.Empty;
            _startValue = default(double);
            _endValue = default(double);
        }

        /// <summary>
        /// Index of the parameter in discussion
        /// </summary>
        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        /// <summary>
        /// Description of the Parameter
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        /// <summary>
        /// Start point of parameter range
        /// </summary>
        public double StartValue
        {
            get { return _startValue; }
            set { _startValue = value; }
        }

        /// <summary>
        /// End point of parameter range
        /// </summary>
        public double EndValue
        {
            get { return _endValue; }
            set { _endValue = value; }
        }
    }
}
