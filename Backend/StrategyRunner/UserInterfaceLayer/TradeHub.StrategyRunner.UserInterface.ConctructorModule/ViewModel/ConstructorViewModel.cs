using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TradeHub.StrategyRunner.UserInterface.Common;
using TradeHub.StrategyRunner.UserInterface.ConctructorModule.View;

namespace TradeHub.StrategyRunner.UserInterface.ConctructorModule.ViewModel
{
    /// <summary>
    /// Provides backend functionality for <see cref="ConstructorView"/>
    /// </summary>
    public class ConstructorViewModel : ViewModelBase
    {
        private Type _type = typeof (ConstructorViewModel);

        /// <summary>
        /// Holds reference to UI dispatcher
        /// </summary>
        private readonly Dispatcher _currentDispatcher;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ConstructorViewModel()
        {
            _currentDispatcher = Dispatcher.CurrentDispatcher;
        }
    }
}
