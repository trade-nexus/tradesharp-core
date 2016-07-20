using System.Windows;
using System.Windows.Controls;
using Spring.Context.Support;
using TradeHub.DataDownloader.UserInterface.DataModule.ViewModel;

namespace TradeHub.DataDownloader.UserInterface.DataModule.View
{
    /// <summary>
    /// Interaction logic for DataView.xaml
    /// </summary>
    public partial class DataView : UserControl
    {
        public DataView(DataViewModel dataViewModel)
        {
            InitializeComponent();
            this.DataContext = dataViewModel;
        }

    }
}
