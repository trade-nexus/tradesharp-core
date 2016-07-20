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
using TradeHub.StrategyRunner.UserInterface.OptimizedStatsModule.ViewModel;

namespace TradeHub.StrategyRunner.UserInterface.OptimizedStatsModule.View
{
    /// <summary>
    /// Interaction logic for OptimizationStatsView.xaml
    /// </summary>
    public partial class OptimizationStatsView : UserControl
    {
        public OptimizationStatsView(OptimizationStatsViewModel optimizationStatsViewModel)
        {
            InitializeComponent();
            DataContext = optimizationStatsViewModel;
        }
    }
}
