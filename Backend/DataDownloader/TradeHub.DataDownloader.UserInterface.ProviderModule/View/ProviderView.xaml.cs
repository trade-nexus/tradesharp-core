
using TradeHub.DataDownloader.UserInterface.ProviderModule.ViewModel;

namespace TradeHub.DataDownloader.UserInterface.ProviderModule.View
{
    /// <summary>
    /// Interaction logic for ProviderView.xaml
    /// </summary>
    public partial class ProviderView
    {
        public ProviderView(ProviderViewModel providerViewModel)
        {
            InitializeComponent();
            DataContext = providerViewModel;
        }

    }
}
