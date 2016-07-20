using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;

namespace TradeHub.Common.Core.DomainModels.OrderDomain
{
    /// <summary>
    /// Contains Complete Trade Information i.e Opening a Position and then Closing it.
    /// </summary>
    [Serializable]
    public class Trade
    {
        private Type _type = typeof (Trade);

        /// <summary>
        /// Unique ID to identify Trade
        /// </summary>
        private string _id;

        /// <summary>
        /// Represents the side which was responsible for starting the Trade
        /// </summary>
        private TradeSide _tradeSide;

        /// <summary>
        /// Size of Trade depending upon the Order Size which strated the Trade
        /// </summary>
        private int _tradeSize;

        /// <summary>
        /// Order Execution Provider On which the executions take place
        /// </summary>
        private string _executionProvider;

        /// <summary>
        /// Security/Symbol for which the Order Executions are occuring
        /// </summary>
        private Security _security;

        /// <summary>
        /// Time when the Trade started
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// Time when the Trade Completed
        /// </summary>
        private DateTime _completionTime;

        /// <summary>
        /// Profit & Loss for the completed Trade
        /// </summary>
        private decimal _profitAndLoss;

        /// <summary>
        /// Position for the current open Trade
        /// </summary>
        private int _position = 0;

        /// <summary>
        /// Contains Execution Details which make up the Trade
        /// KEY = Execution IDs
        /// VALUE = Execution Size used in the current Trade for closing it
        /// </summary>
        private IDictionary<string, int> _executionDetails;

        /// <summary>
        /// Unique ID to identify Trade
        /// </summary>
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        /// <summary>
        /// Represents the side which was responsible for starting the Trade
        /// </summary>
        public TradeSide TradeSide
        {
            get { return _tradeSide; }
            set { _tradeSide = value; }
        }

        /// <summary>
        /// Size of Trade depending upon the Order Size which strated the Trade
        /// </summary>
        public int TradeSize
        {
            get { return _tradeSize; }
            set { _tradeSize = value; }
        }

        /// <summary>
        /// Order Execution Provider On which the executions take place
        /// </summary>
        public string ExecutionProvider
        {
            get { return _executionProvider; }
            set { _executionProvider = value; }
        }

        /// <summary>
        /// Contains Execution Details which make up the Trade
        /// KEY = Execution IDs
        /// VALUE = Execution Size used in the current Trade for closing it
        /// </summary>
        public IDictionary<string, int> ExecutionDetails
        {
            get { return _executionDetails; }
            set { _executionDetails = value; }
        }

        /// <summary>
        /// Security/Symbol for which the Order Executions are occuring
        /// </summary>
        public Security Security
        {
            get { return _security; }
            set { _security = value; }
        }

        /// <summary>
        /// Time when the Trade started
        /// </summary>
        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }

        /// <summary>
        /// Time when the Trade Completed
        /// </summary>
        public DateTime CompletionTime
        {
            get { return _completionTime; }
            set { _completionTime = value; }
        }

        /// <summary>
        /// Profit & Loss for the completed Trade
        /// </summary>
        public decimal ProfitAndLoss
        {
            get { return _profitAndLoss; }
            set { _profitAndLoss = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public Trade()
        {

        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="tradeSide">Represents the side which was responsible for starting the Trade</param>
        /// <param name="tradeSize">Size of Trade depending upon the Order Size which strated the Trade</param>
        /// <param name="executionPrice">Price at which the order is executed</param>
        /// <param name="executionProvider">Order Execution Provider On which the executions take place</param>
        /// <param name="executionId">Execution ID for Order which initiated the Trade</param>
        /// <param name="security">Security/Symbol for which the Order Executions are occuring</param>
        /// <param name="startTime">Execution time to be used for Trade Start Time</param>
        public Trade(TradeSide tradeSide, int tradeSize, decimal executionPrice, string executionProvider, string executionId, Security security, DateTime startTime)
        {
            // Save Information
            _tradeSide = tradeSide;
            _tradeSize = tradeSize;
            _executionProvider = executionProvider;
            _security = security;
            _startTime = startTime;
            
            // Initialize
            _executionDetails = new Dictionary<string, int>();

            // Set initial Position value
            _position = tradeSize;

            // Set initial PnL value
            _profitAndLoss = executionPrice * tradeSize;

            // Set value to 'Negative' if the Trade side is 'SELL'
            if (tradeSide.Equals(Constants.TradeSide.Sell))
            {
                _position *= -1;
                _tradeSize *= -1;
                _profitAndLoss *= -1;
            }

            // Add initial values to the local Map
            _executionDetails.Add(executionId, _position);
        }

        /// <summary>
        /// Add new execution details to the current Trade
        /// </summary>
        /// <param name="executionId">Unique Execution ID</param>
        /// <param name="executionSize">Shares filled in the current execution</param>
        /// <param name="executionPrice">Price at which the order is executed</param>
        /// <param name="executionTime">Execution Time</param>
        /// <returns>Remaining amount from the Execution Size after adding it into the current Trade</returns>
        public int Add(string executionId, int executionSize, decimal executionPrice, DateTime executionTime)
        {
            // Process only if the Trade is still Open
            if (!IsComplete())
            {
                // Process Execution Size to be added to the current Trade
                // If the start Trade is BUY incoming executions will be 'Negative'
                if (_tradeSide.Equals(Constants.TradeSide.Buy))
                {
                    executionSize *= -1;
                }

                // Get remaining Position size
                int remainingPosition = _position + executionSize;

                // Check if the Trade is complete.
                // Check if incoming execution can be fully utilized.
                if (remainingPosition == 0 || HaveSameSign(_position, remainingPosition))
                {
                    // Trade is complete if Remaining Position equals '0' else Update the current Position
                    _position = remainingPosition;

                    // Add Execution Details to local Map
                    _executionDetails.Add(executionId, executionSize);

                    // Update PnL value
                    _profitAndLoss += (executionPrice * executionSize);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Trade Updated with Size: " + executionSize + " ExecutionID: " + executionId, _type.FullName, "Add");
                        Logger.Debug(this.ToString(), _type.FullName, "Add");
                    }

                    // Check if the Trade is complete
                    if (IsComplete())
                    {
                        // Set Execution Time as Completion for the Trade
                        _completionTime = executionTime;
                    }

                    // Indicates the incoming execution size was fully utilized
                    return 0;
                }
                // Incoming execution is more than whats need to complete the trade
                else
                {
                    // Get execution size which can be utilized
                    int executionSizeToUse = Math.Abs(executionSize) - (Math.Abs(remainingPosition));

                    // Update Position
                    _position = 0;

                    // If the start Trade is BUY execution size to use will be 'Negative'
                    if (_tradeSide.Equals(Constants.TradeSide.Buy))
                    {
                        executionSizeToUse *= -1;
                    }

                    // Add Execution Details to local Map
                    _executionDetails.Add(executionId, executionSizeToUse);

                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("Trade Updated with Size: " + executionSize + " ExecutionID: " + executionId, _type.FullName, "Add");
                        Logger.Debug(this.ToString(), _type.FullName, "Add");
                    }

                    // Update PnL value
                    _profitAndLoss += (executionPrice * executionSizeToUse);

                    // Check if the Trade is complete
                    if (IsComplete())
                    {
                        // Set Execution Time as Completion for the Trade
                        _completionTime = executionTime;
                    }

                    // Return the execution size which was not utilized
                    return Math.Abs(remainingPosition);
                }
            }

            // Indicates nothing was utilized
            return executionSize;
        }

        /// <summary>
        /// Indicates if the current Trade is Complete or still Open
        /// </summary>
        /// <returns>Bool indicator representing Trade completion State</returns>
        public bool IsComplete()
        {
            // Trade is complete when the Position has reached '0'
            return _position == 0;
        }

        /// <summary>
        /// Checks whether the two integers have same sign (positive or negative)
        /// </summary>
        /// <param name="x">First parameter</param>
        /// <param name="y">Second parameter</param>
        /// <returns>Bool value indicating if both values had the same sign</returns>
        private bool HaveSameSign(int x, int y)
        {
            return ((x < 0) == (y < 0));
        }

        /// <summary>
        /// ToString Override for Trade.cs
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("Trade::");
            stringBuilder.Append(" Side: " + _tradeSide);
            stringBuilder.Append(" Size: " + _tradeSize);
            stringBuilder.Append(" Closed: " + IsComplete());
            stringBuilder.Append(" Start Time: " + _startTime);
            stringBuilder.Append(" Completion Time: " + _completionTime);
            stringBuilder.Append(" " + _security);

            return stringBuilder.ToString();
        }
    }
}
