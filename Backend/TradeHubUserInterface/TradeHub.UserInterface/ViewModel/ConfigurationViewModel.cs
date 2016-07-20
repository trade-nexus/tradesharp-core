using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TraceSourceLogger;
using TradeHub.UserInterface.Common;
using TradeHub.UserInterface.Infrastructure.ProvidersConfigurations;
using TradeHub.UserInterface.ServicesModule.Commands;

namespace TradeHub.UserInterface.ServicesModule.ViewModel
{
    public class ConfigurationViewModel:DependencyObject
    {
        public ObservableCollection<Parameters> ProviderParameterses;

        public List<string> ProviderList;

        public static readonly DependencyProperty SelectedProviderProperty =
            DependencyProperty.Register("SelectedProvider", typeof (string), typeof (ConfigurationViewModel), new PropertyMetadata(default(string)));

        public string SelectedProvider
        {
            get { return (string) GetValue(SelectedProviderProperty); }
            set { SetValue(SelectedProviderProperty, value); }
        }

        private string _serviceName;

        public ICommand SaveCommand { get; set; }

        private Dispatcher _currentDispatcher;
        public ConfigurationViewModel(string serviceName)
        {
            _currentDispatcher = Dispatcher.CurrentDispatcher;
            _serviceName = serviceName;
            SaveCommand=new SaveCommand(this);
            ProviderParameterses=new ObservableCollection<Parameters>();
            ProviderList=new List<string>();
            ProviderList.Add("Blackwood");
            SelectedProvider = "Blackwood";
            EventSystem.Subscribe<List<Parameters>>(ReceivedParameters);
            LoadProviderParameters();
            
        }

        /// <summary>
        /// Function to request the call for loading parameters paramters
        /// </summary>
        private void LoadProviderParameters()
        {
            ServiceProvider provider=new ServiceProvider(_serviceName,SelectedProvider);
            EventSystem.Publish<ServiceProvider>(provider);
            
        }

        /// <summary>
        /// Receive providers parameters
        /// </summary>
        /// <param name="parameterses"></param>
        private void ReceivedParameters(List<Parameters> parameterses)
        {
            Logger.Info("Receieved parameters List","","");
            if (parameterses != null)
            {
                _currentDispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
                {
                    ProviderParameterses.Clear();

                    foreach (var parameterse in parameterses)
                    {
                        ProviderParameterses.Add(parameterse);
                    }

                }));
            }
        }

        /// <summary>
        /// Save the parameters.
        /// </summary>
        public void SaveParameters()
        {
            ServiceParametersList parametersList=new ServiceParametersList(new ServiceProvider(_serviceName,SelectedProvider),ProviderParameterses.ToList());
            EventSystem.Publish(parametersList);
            MessageBox.Show("Please start the service to load new configuration");


        }
    }
}
