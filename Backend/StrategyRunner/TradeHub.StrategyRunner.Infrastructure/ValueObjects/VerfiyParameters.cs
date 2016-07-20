using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeHub.StrategyRunner.Infrastructure.ValueObjects
{
    /// <summary>
    /// Contains info for the constructor parameters to be loaded
    /// </summary>
    public class VerfiyParameters
    {
        /// <summary>
        /// Key/Index of the loaded parameters
        /// </summary>
        private int _selectedIndex;

        /// <summary>
        /// Arguments to be verified
        /// </summary>
        private object[] _ctrArgs;

        /// <summary>
        /// Key/Index of the loaded parameters
        /// </summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; }
        }

        /// <summary>
        /// Arguments to be verified
        /// </summary>
        public object[] CtrArgs
        {
            get { return _ctrArgs; }
            set { _ctrArgs = value; }
        }

        /// <summary>
        /// Argument Constructor
        /// </summary>
        /// <param name="selectedIndex">Key/Index of the loaded parameters</param>
        public VerfiyParameters(int selectedIndex)
        {
            _selectedIndex = selectedIndex;
        }
    }
}
