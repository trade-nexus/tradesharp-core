
using System.ComponentModel;
using System.Windows;
using Spring.Context.Support;
using TradeHub.DataDownloader.UserInterface.DataModule.ViewModel;

namespace TradeHub.DataDownloader.UserInterface.DataModule.View
{
    /// <summary>
    /// Interaction logic for BarSettingView.xaml
    /// </summary>
    public partial class BarSettingView : Window
    {
        public BarSettingViewModel BarSettingViewModel;
        
        public BarSettingView()
        {
            InitializeComponent();
            var context = ContextRegistry.GetContext();
            BarSettingViewModel = context.GetObject("BarSettingViewModel") as BarSettingViewModel;
            this.DataContext = BarSettingViewModel;
        }

        /// <summary>
        /// On Close Event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
            Hide();
        }

        /// <summary>
        /// On Save Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSave(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
