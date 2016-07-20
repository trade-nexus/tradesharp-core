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
using System.Windows.Shapes;
using TradeHub.UserInterface.ServicesModule.ViewModel;

namespace TradeHub.UserInterface.ServicesModule.View
{
    /// <summary>
    /// Interaction logic for ConfigurationView.xaml
    /// </summary>
    public partial class ConfigurationView : Window
    {
        public ConfigurationView(string serviceName)
        {
            InitializeComponent();
            ConfigurationViewModel viewModel=new ConfigurationViewModel(serviceName);
            this.DataContext = viewModel;
            ParametersGrid.ItemsSource = viewModel.ProviderParameterses;
            ProviderList.ItemsSource = viewModel.ProviderList;
           
        }

        
    }
}
