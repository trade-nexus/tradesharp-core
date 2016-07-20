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
using TradeHub.UserInterface.ComponentsModule.ViewModels;

namespace TradeHub.UserInterface.ComponentsModule.Views
{
    /// <summary>
    /// Interaction logic for ComponentsView.xaml
    /// </summary>
    public partial class ComponentsView : UserControl
    {
        private ComponentsViewModel _viewModel;
        public ComponentsView()
        {
            InitializeComponent();
            _viewModel=new ComponentsViewModel();
            this.DataContext = _viewModel;
        }
    }
}
