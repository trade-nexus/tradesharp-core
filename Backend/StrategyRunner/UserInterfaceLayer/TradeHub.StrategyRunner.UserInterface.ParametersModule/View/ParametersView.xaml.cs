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
using TradeHub.StrategyRunner.UserInterface.ParametersModule.ViewModel;

namespace TradeHub.StrategyRunner.UserInterface.ParametersModule.View
{
    /// <summary>
    /// Interaction logic for ParametersView.xaml
    /// </summary>
    public partial class ParametersView : UserControl
    {
        public ParametersView(ParametersViewModel parametersViewModel)
        {
            InitializeComponent();
            DataContext = parametersViewModel;
        }
    }
}
