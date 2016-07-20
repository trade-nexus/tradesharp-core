using System.Windows.Controls;
using TradeHub.UserInterface.ServicesModule.ViewModel;

namespace TradeHub.UserInterface.ServicesModule.View
{
    /// <summary>
    /// Interaction logic for TradeHubServicesView.xaml
    /// </summary>
    public partial class TradeHubServicesView : UserControl
    {
        private TradeHubServicesViewModel _viewModel;
        public TradeHubServicesView()
        {
            InitializeComponent();
            _viewModel=new TradeHubServicesViewModel();
           this.DataContext = _viewModel;
            
        }
    }
}
