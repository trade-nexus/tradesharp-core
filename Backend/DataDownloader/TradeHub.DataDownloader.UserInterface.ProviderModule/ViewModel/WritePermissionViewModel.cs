using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using TradeHub.Common.Core.Constants;
using TradeHub.DataDownloader.Common.ConcreteImplementation;
using TradeHub.DataDownloader.UserInterface.Common;
using TradeHub.DataDownloader.UserInterface.Common.Messages;

namespace TradeHub.DataDownloader.UserInterface.ProviderModule.ViewModel
{
    public class WritePermissionViewModel:ViewModelBase
    {
        private string _selectedProvider;
        /// <summary>
        /// Binds to Csv Check Box
        /// </summary>
        private bool _writeToCsv;
        public bool WriteToCsv
        {
            get { return _writeToCsv; }
            set
            {
                _writeToCsv = value;
                RaisePropertyChanged("WriteToCsv");
            }
        }

        /// <summary>
        /// Binds to Csv Check Box
        /// </summary>
        private bool _writeToBinary;
        public bool WriteToBinary
        {
            get { return _writeToBinary; }
            set
            {
                _writeToBinary = value;
                RaisePropertyChanged("WriteToBinary");
            }
        }

        /// <summary>
        /// Binds to Csv Check Box
        /// </summary>
        private bool _writeToDatabase;
        public bool WriteToDatabase
        {
            get { return _writeToDatabase; }
            set
            {
                _writeToDatabase = value;
                RaisePropertyChanged("WriteToDatabase");
            }
        }

        /// <summary>
        /// Command To provide Save Action
        /// </summary>
        public ICommand Save { get; set; }
        public WritePermissionViewModel()
        {
            Save=new DelegateCommand(SaveAction);
            EventSystem.Subscribe<SelectedProviderFromList>(SelectedProvider);
        }

        /// <summary>
        /// Methord fired then user Click Save button
        /// It Over Writes the Broker Permissions
        /// </summary>
        private void SaveAction()
        {
            EventSystem.Publish<ProviderPermission>(new ProviderPermission
                {
                    MarketDataProvider = _selectedProvider,
                    WriteCsv = WriteToCsv,
                    WriteBinary = WriteToBinary,
                    WriteDatabase = WriteToDatabase
                });
        }

        /// <summary>
        /// Current Selected Provider
        /// </summary>
        /// <param name="providerFromList"></param>
        private void SelectedProvider(SelectedProviderFromList providerFromList)
        {
            _selectedProvider = providerFromList.ProviderName;
        }

    }
}
