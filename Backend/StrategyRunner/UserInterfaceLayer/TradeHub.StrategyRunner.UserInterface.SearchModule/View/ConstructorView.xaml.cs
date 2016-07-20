using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Spring.Context.Support;
using TradeHub.StrategyRunner.UserInterface.SearchModule.ViewModel;
using TradeHubConstants = TradeHub.Common.Core.Constants;

namespace TradeHub.StrategyRunner.UserInterface.SearchModule.View
{
    /// <summary>
    /// Interaction logic for ConstructorView.xaml
    /// </summary>
    public partial class ConstructorView : Window
    {
        private readonly GridView _myGridView = new GridView();

        private readonly DataTable _dataTable= new DataTable();

        // Elements to search for Market Data Provider Column
        private string[] _marketDataProviderSearch = new string[]
            {
                "market", "data", "live"
            };

        // Elements to search for Historical Data Provider Column
        private string[] _historicalDataProviderSearch = new string[]
            {
                "historic", "data", "historical"
            };

        // Elements to search for Order Execution Provider Column
        private string[] _orderExecutionProviderSearch = new string[]
            {
                "order", "execution"
            };


        public ConstructorViewModel ConstructorViewModel;

        public ConstructorView()
        {
            InitializeComponent();
            var context = ContextRegistry.GetContext();
            ConstructorViewModel = context.GetObject("ConstructorViewModel") as ConstructorViewModel;
            this.DataContext = ConstructorViewModel;
            this.ConstructorDataGrid.DataContext = _dataTable;
        }

        /// <summary>
        /// Adds Column headers to the ListView for Constructor Details
        /// </summary>
        /// <param name="parameterInfoArray"></param>
        public void SetGridColumnHeader(ParameterInfo[] parameterInfoArray)
        {
            double windowWidth = 0;
            foreach (ParameterInfo info in parameterInfoArray)
            {
                // Add new column
                //if (IsProviderColumn(info.Name))
                //{
                //    var dgCmbCol = new DataGridComboBoxColumn();
                //    dgCmbCol.Header = info.Name;

                //    if (IsMarketProviderColumn(info.Name))
                //    {
                //        dgCmbCol.ItemsSource = ListOfMarketProviders();
                //    }
                //    else if (IsHistoricalProviderColumn(info.Name))
                //    {
                //        dgCmbCol.ItemsSource = ListOfMarketProviders();
                //    }
                //    else if (IsOrderProviderColumn(info.Name))
                //    {
                //        dgCmbCol.ItemsSource = ListOfOrderExectuionProviders();
                //    }
                //    else
                //    {
                //        dgCmbCol.ItemsSource = ListOfMarketProviders();
                //    }

                //    _dataTable.Columns.Add(dgCmbCol);
                //}
                //else
                {
                    DataColumn dataColumn = new DataColumn();
                    dataColumn.ColumnName = info.Name;
                    _dataTable.Columns.Add(dataColumn);
                }

                // Calculate window length
                windowWidth += info.Name.Length * 8;
            }
            
            // Change window width to show parameters
            if (windowWidth + 15 > 400 && windowWidth + 15 < 1020)
            {
                this.Width = windowWidth + 15;
            }
            else if (windowWidth + 15 > 1020)
            {
                this.Width = 1020;
            }
            else
            {
                this.Width = 400;
            }
        }

        /// <summary>
        /// Add Constuctor Parameter values
        /// </summary>
        /// <param name="parameters"></param>
        public void SetGridColumnValues(string[] parameters)
        {
            DataRow row = _dataTable.NewRow();
            for (int i = 0; i < parameters.Length; i++)
            {
                row[i] = parameters[i];
            }
            _dataTable.Rows.Add(row);
        }

        /// <summary>
        /// Called when RUN button is clicked
        /// </summary>
        private void OnRunButtonClick(object sender, RoutedEventArgs e)
        {
            var items = ConstructorDataGrid.SelectedItems;
            foreach (DataRowView item in items)
            {
                ConstructorViewModel.RunStrategy(item.Row.ItemArray);
            }
            //var index = ConstructorDataGrid.SelectedIndex;
            //ConstructorViewModel.RunStrategy(index);
        }

        /// <summary>
        /// Called when BRUTE OPTIMIZATION button is clicked
        /// </summary>
        private void OnOptimizeButtonClick(object sender, RoutedEventArgs e)
        {
            var index = ConstructorDataGrid.SelectedIndex;
            ConstructorViewModel.OptimizeStrategyBruteForce(index);
        }

        /// <summary>
        /// Called when GENETIC OPTIMIZATION button is clicked
        /// </summary>
        private void OnGeneticOptimzeButtonClick(object sender, RoutedEventArgs e)
        {
            DataRowView item = (DataRowView)ConstructorDataGrid.SelectedItems[0];
            ConstructorViewModel.OptimizeStrategyGeneticAlgorithm(item.Row.ItemArray);

            //var index = ConstructorDataGrid.SelectedIndex;
            //ConstructorViewModel.OptimizeStrategyGeneticAlgorithm(index);
        }

        /// <summary>
        /// Checks if the value to be added is a Provider Name
        /// </summary>
        /// <param name="value">Value to check</param>
        private bool IsProviderColumn(string value)
        {
            // Check if the value is intended to be provider
            if (value.ToLower().Contains("provider"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if the value to be added is a Market Provider Name
        /// </summary>
        /// <param name="input">Value to check</param>
        private bool IsMarketProviderColumn(string input)
        {
            return _marketDataProviderSearch.Any(x => input.Contains(x));
        }

        /// <summary>
        /// Checks if the value to be added is a Historical Provider Name
        /// </summary>
        /// <param name="input">Value to check</param>
        private bool IsHistoricalProviderColumn(string input)
        {
            return _historicalDataProviderSearch.Any(x => input.Contains(x));
        }

        /// <summary>
        /// Checks if the value to be added is a Order Provider Name
        /// </summary>
        /// <param name="input">Value to check</param>
        private bool IsOrderProviderColumn(string input)
        {
            return _orderExecutionProviderSearch.Any(x => input.Contains(x));
        }

        /// <summary>
        /// Provides a list containing Market Data Provider Names
        /// </summary>
        /// <returns></returns>
        private List<string> ListOfMarketProviders()
        {
            return new List<string>()
                {
                    TradeHubConstants.MarketDataProvider.Blackwood,
                    TradeHubConstants.MarketDataProvider.Simulated,
                    TradeHubConstants.MarketDataProvider.InteractiveBrokers,
                    TradeHubConstants.MarketDataProvider.SimulatedExchange
                };
        }

        /// <summary>
        /// Provides a list containing Order Execution Provider Names
        /// </summary>
        /// <returns></returns>
        private List<string> ListOfOrderExectuionProviders()
        {
            return new List<string>()  {
                    TradeHubConstants.OrderExecutionProvider.Blackwood,
                    TradeHubConstants.OrderExecutionProvider.Simulated,
                    TradeHubConstants.OrderExecutionProvider.SimulatedExchange
                };
        }
    }
}
