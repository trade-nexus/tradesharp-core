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
using TradeHub.StrategyRunner.UserInterface.GaParametersModule.ViewModel;

namespace TradeHub.StrategyRunner.UserInterface.GaParametersModule.View
{
    /// <summary>
    /// Interaction logic for GaParametersView.xaml
    /// </summary>
    public partial class GaParametersView : UserControl
    {
        public GaParametersView(GaParametersViewModel gaParametersViewModel)
        {
            InitializeComponent();
            DataContext = gaParametersViewModel;
        }
    }
}
