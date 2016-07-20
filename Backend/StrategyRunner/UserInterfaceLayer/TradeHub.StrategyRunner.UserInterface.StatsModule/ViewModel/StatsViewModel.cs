using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Practices.Unity;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels.OrderDomain;
using TradeHub.StrategyRunner.Infrastructure.Entities;
using TradeHub.StrategyRunner.Infrastructure.ValueObjects;
using TradeHub.StrategyRunner.UserInterface.Common;
using TradeHub.StrategyRunner.UserInterface.Common.Utility;
using TradeHub.StrategyRunner.UserInterface.Common.ValueObjects;
using TradeHub.StrategyRunner.UserInterface.StatsModule.Utility;

namespace TradeHub.StrategyRunner.UserInterface.StatsModule.ViewModel
{
    /// <summary>
    /// Provides backend functionality for StatsView.xaml
    /// </summary>
    public class StatsViewModel : ViewModelBase
    {
        private Type _type = typeof (StatsViewModel);

        /// <summary>
        /// Holds reference to UI dispatcher
        /// </summary>
        private readonly Dispatcher _currentDispatcher;

        /// <summary>
        /// Contains all the Order Executions 
        /// </summary>
        private ObservableCollection<Execution> _ordersCollection;

        /// <summary>
        /// Contains all the Order Executions 
        /// </summary>
        public ObservableCollection<Execution> OrdersCollection
        {
            get { return _ordersCollection; }
            set { _ordersCollection = value; }
        }

        private Statistics _statistics=new Statistics("A00");
        ///// <summary>
        ///// Contains all the Order Executions 
        ///// </summary>
        //private AsyncVirtualizingCollection<Execution> _ordersCollection;

        ///// <summary>
        ///// Contains all the Order Executions 
        ///// </summary>
        //public AsyncVirtualizingCollection<Execution> OrdersCollection
        //{
        //    get { return _ordersCollection; }
        //    set { _ordersCollection = value; }
        //}

        /// <summary>
        /// Default Constructor
        /// </summary>
        public StatsViewModel()
        {
            this._currentDispatcher = Dispatcher.CurrentDispatcher;

            //_ordersCollection = new AsyncVirtualizingCollection<Execution>(new ExecutionCollection(), 100);
            _ordersCollection = new ObservableCollection<Execution>();
            
            // Register Event Aggregator
            EventSystem.Subscribe<UpdateStats>(UpdateCustomStrategyStats);
            EventSystem.Subscribe<string>(ManageUserCommands);
        }

        /// <summary>
        /// Manages incoming general user commands
        /// </summary>
        /// <param name="value"></param>
        private void ManageUserCommands(string value)
        {
            if (value.Equals("ExportOrders"))
            {
                ExportUserOrders(value);
            }
        }

        /// <summary>
        /// Update Order Execution Stats on UI on new execution event
        /// </summary>
        /// <param name="updateStats"></param>
        private void UpdateCustomStrategyStats(UpdateStats updateStats)
        {
            try
            {
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Updating execution stats for: " + updateStats.Execution.Order.OrderID, _type.FullName, "UpdateCustomStrategyStats");
                }
                _statistics.UpdateCalulcationsOnExecutionMatlab(updateStats.Execution);
                //if (_ordersCollection.Count < 1000)
                {
                    _currentDispatcher.Invoke(DispatcherPriority.Background, (Action) (() =>
                        {
                            //_ordersCollection.Insert(0, updateStats.Execution);
                            _ordersCollection.Add(updateStats.Execution);
                        }));
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "UpdateCustomStrategyStats");
            }
        }

        /// <summary>
        /// Exports all user order to CSV file
        /// </summary>
        /// <param name="value"></param>
        private void ExportUserOrders(string value)
        {
            try
            {
                IUnityContainer container = new UnityContainer();
                var folderDialogService = container.Resolve<FolderDialogService>();
                if (folderDialogService.OpenFolderDialog() == true)
                {
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug("File select browser opened.", _type.FullName, "Export");
                    }
                    String folderName = folderDialogService.FolderName;
                    FileWriter.WriteFile(folderName, _ordersCollection);
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Number of instruments loaded: " + this._ordersCollection.Count, _type.FullName, "Export");
                    }
                }
                MessageBox.Show("Risk=" + _statistics.GetRisk());
                _statistics.ResetAllValues();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Export");
            }
        }
    }
}
