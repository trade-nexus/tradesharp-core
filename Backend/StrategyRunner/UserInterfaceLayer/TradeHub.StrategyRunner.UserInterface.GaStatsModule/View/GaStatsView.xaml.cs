using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TradeHub.StrategyRunner.UserInterface.GaStatsModule.ViewModel;

namespace TradeHub.StrategyRunner.UserInterface.GaStatsModule.View
{
    /// <summary>
    /// Interaction logic for GaStatsView.xaml
    /// </summary>
    public partial class GaStatsView : UserControl
    {
        public GaStatsView(GaStatsViewModel gaStatsViewModel)
        {
            InitializeComponent();
            DataContext = gaStatsViewModel;
            dataGrid1.ItemsSource = gaStatsViewModel.ParametersInfo;
            //listView1.DataContext = gaStatsViewModel.ParametersInfo;
            //listView1.ItemsSource = gaStatsViewModel.ParametersInfo;
        }
    }
}
