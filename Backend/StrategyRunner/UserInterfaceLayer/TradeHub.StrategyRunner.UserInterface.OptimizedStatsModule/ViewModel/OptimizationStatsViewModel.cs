using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TraceSourceLogger;
using TradeHub.StrategyRunner.Infrastructure.Entities;
using TradeHub.StrategyRunner.UserInterface.Common;

namespace TradeHub.StrategyRunner.UserInterface.OptimizedStatsModule.ViewModel
{
    /// <summary>
    /// Provides backend fucntionality for OptimizationStatsView.xaml
    /// </summary>
    public class OptimizationStatsViewModel : ViewModelBase
    {
        private Type _type = typeof (OptimizationStatsViewModel);

        /// <summary>
        /// Holds reference to UI dispatcher
        /// </summary>
        private readonly Dispatcher _currentDispatcher;

        /// <summary>
        /// Contains statistics for all the optimization iterations
        /// </summary>
        private ObservableCollection<OptimizationStatistics> _statisticsCollection;

        /// <summary>
        /// Contains statistics for all the optimization iterations
        /// </summary>
        public ObservableCollection<OptimizationStatistics> StatisticsCollection
        {
            get { return _statisticsCollection; }
            set { _statisticsCollection = value; }
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public OptimizationStatsViewModel()
        {
            _currentDispatcher = Dispatcher.CurrentDispatcher;
            _statisticsCollection= new ObservableCollection<OptimizationStatistics>();

            // Event Aggregator
            EventSystem.Subscribe<OptimizationStatistics>(DisplayOptimizationStatistics);
        }

        /// <summary>
        /// Displays the optimization statistics from each iteration on UI
        /// </summary>
        /// <param name="optimizationStatistics">Contains info for the given iterations execution statistics</param>
        [MethodImpl(MethodImplOptions.Synchronized)] 
        private void DisplayOptimizationStatistics(OptimizationStatistics optimizationStatistics)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Displaying optimization statistics", _type.FullName, "DisplayOptimizationStatistics");
                }

                // Display on UI
                _currentDispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => StatisticsCollection.Add(optimizationStatistics)));
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "DisplayOptimizationStatistics");
            }
        }
    }
}
