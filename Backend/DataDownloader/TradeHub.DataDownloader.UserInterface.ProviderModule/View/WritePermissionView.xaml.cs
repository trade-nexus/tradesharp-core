using System.ComponentModel;
using System.Windows;
using TradeHub.DataDownloader.UserInterface.ProviderModule.ViewModel;

namespace TradeHub.DataDownloader.UserInterface.ProviderModule.View
{
    /// <summary>
    /// Interaction logic for WritePermissionView.xaml
    /// </summary>
    public partial class WritePermissionView : Window
    {
        public WritePermissionView(WritePermissionViewModel permissionViewModel)
        {
            InitializeComponent();
            DataContext = permissionViewModel;
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            Hide();
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
