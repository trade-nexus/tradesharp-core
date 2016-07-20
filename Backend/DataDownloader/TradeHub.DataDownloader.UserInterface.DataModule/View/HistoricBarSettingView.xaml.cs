using System.ComponentModel;
using System.Windows;
using Spring.Context.Support;
using TradeHub.DataDownloader.UserInterface.DataModule.ViewModel;

namespace TradeHub.DataDownloader.UserInterface.DataModule.View
{
    /// <summary>
    /// Interaction logic for HistoricBarSettingView.xaml
    /// </summary>
    public partial class HistoricBarSettingView : Window
    {
        public HistoricBarViewModel HistoricBarViewModel;
        public HistoricBarSettingView()
        {
            InitializeComponent();
            var context = ContextRegistry.GetContext();
            HistoricBarViewModel = context.GetObject("HistoricBarViewModel") as HistoricBarViewModel;
            this.DataContext = HistoricBarViewModel;
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
        private void OnSubmit(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
