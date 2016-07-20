using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains info to identify the selected user strategy
    /// </summary>
    public class SelectedStrategy
    {
        /// <summary>
        /// Unique key to identify strategy
        /// </summary>
        private string _key;

        /// <summary>
        /// Symbol on which the strategy is executing
        /// </summary>
        private string _symbol;

        /// <summary>
        /// Brief info related to strategy parameters
        /// </summary>
        private string _briefInfo;

        /// <summary>
        /// Indicates whether the strategy is running/stopped
        /// </summary>
        private bool _isRunning;

        /// <summary>
        /// Unique key to identify strategy
        /// </summary>
        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        /// <summary>
        /// Symbol on which the strategy is executing
        /// </summary>
        public string Symbol
        {
            get { return _symbol; }
            set { _symbol = value; }
        }

        /// <summary>
        /// Brief info related to strategy parameters
        /// </summary>
        public string BriefInfo
        {
            get { return _briefInfo; }
            set { _briefInfo = value; }
        }

        /// <summary>
        /// Indicates whether the strategy is running/stopped
        /// </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; }
        }
        
        /// <summary>
        /// ToString override for the SelectedStrategy.cs
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder stringBuilder= new StringBuilder();

            stringBuilder.Append("Selected Strategy :: ");
            stringBuilder.Append("Key: " + _key);
            stringBuilder.Append(" | Symbol: " + _symbol);
            stringBuilder.Append(" | Brief Info: " + _briefInfo);

            return stringBuilder.ToString();
        }
    }
}
