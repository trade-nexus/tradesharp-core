using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.DataDownloader.Common.ConcreteImplementation;
using TradeHub.DataDownloader.UserInterface.Common;
using TradeHub.DataDownloader.UserInterface.Common.Messages;
using TradeHub.DataDownloader.UserInterface.ProviderModule.View;

namespace TradeHub.DataDownloader.UserInterface.ProviderModule.ViewModel
{
    /// <summary>
    ///Provides The Backend Fucntionality to ProviderView
    /// </summary>
    public class ProviderViewModel:ViewModelBase
    {

        /// <summary>
        /// Property value changes when tabs are switched.
        /// </summary>
        private Provider _currentSelectedProviderTab;
        public Provider CurrentSelectedProviderTab
        {
            get { return _currentSelectedProviderTab; }
            set
            {
                _currentSelectedProviderTab = value;
                RaisePropertyChanged("CurrentSelectedProviderTab");
            }
        }

        /// <summary>
        /// Selected Instrument from the List View
        /// </summary>
        private Security _selectedInstrument;
        public Security SelectedInstrument
        {
            get { return _selectedInstrument; }
            set { _selectedInstrument = value; }
        }

        /// <summary>
        /// WritePermissionView Handler
        /// </summary>
        private WritePermissionView _writePermissionView;

        #region Command Region

        /// <summary>
        /// Command To Edit Properties of Data Provider.
        /// Basically user can change the medium where 
        /// to write data of a certain broker
        /// </summary>
        public ICommand EditProperties { get; set; }

        /// <summary>
        /// Command Fired when user Connect To Certain Provider
        /// </summary>
        public ICommand ConnectToProvider { get; set; }

        /// <summary>
        /// Command Fired When User Double Click on Instrument List Item
        /// </summary>
        public ICommand InstrumentSelected { get; set; }

        #endregion

        
        private Type _oType = typeof(ViewModelBase);
        
        /// <summary>
        /// Search String Provided by user to filter results
        /// </summary>
        private string _search;
        public string Search
        {
            get { return _search; }
            set
            {
                _search = value.ToUpper();
                RaisePropertyChanged("Search");
                OnSearchChanged(_search);
            }
        }

        /// <summary>
        /// Selected Provider on the List
        /// </summary>
        private Provider _selectedProvider;
        public Provider SelectedProvider
        {
            get { return _selectedProvider; }
            set
            {
                _selectedProvider = value;
                EventSystem.Publish<SelectedProviderFromList>(new SelectedProviderFromList
                    {ProviderName = _selectedProvider.ProviderName});
                RaisePropertyChanged("SelectedProvider");
            }
        }

        /// <summary>
        /// Collection To Populate Front end With all available Providers
        /// </summary>
        private ObservableCollection<Provider> _providerList;
        public ObservableCollection<Provider> ProviderList
        {
            get { return _providerList; }
            set { _providerList = value; }
        }

        /// <summary>
        /// List of all the current Securities present
        /// </summary>
        private ObservableCollection<Security> _instrumentList;
        public ObservableCollection<Security> InstrumentList
        {
            get { return _instrumentList; }
            set { _instrumentList = value; }
        }

        /// <summary>
        /// List Of filtered Search Query Depending 
        /// on the search entered by the user
        /// </summary>
        private ObservableCollection<Security> _searchResults;
        public ObservableCollection<Security> SearchResults
        {
            get { return _searchResults; }
            set { _searchResults = value; }
        }

        /// <summary>
        /// Class Constructor
        /// </summary>
        public ProviderViewModel(WritePermissionView writePermissionView)
        {
            try
            {
                _writePermissionView = writePermissionView;
                ProviderList=new ObservableCollection<Provider>();
                ProviderList.Add(new Provider{ProviderName = MarketDataProvider.Blackwood,IsConnected = false});
                ProviderList.Add(new Provider { ProviderName = MarketDataProvider.Simulated, IsConnected = false });
                ProviderList.Add(new Provider { ProviderName = MarketDataProvider.InteractiveBrokers, IsConnected = false });

                InstrumentList = new ObservableCollection<Security>();
                InstrumentList.Add(new Security { Symbol = "AAPL" });
                InstrumentList.Add(new Security { Symbol = "GOOG" });
                InstrumentList.Add(new Security { Symbol = "IBM" });
                InstrumentList.Add(new Security { Symbol = "CISCO" });
                InstrumentList.Add(new Security { Symbol = "DELL" });
                InstrumentList.Add(new Security { Symbol = "INTC" });
                InstrumentList.Add(new Security { Symbol = "GE" });
                InstrumentList.Add(new Security { Symbol = "MDY" });
                InstrumentList.Add(new Security { Symbol = "FAS" });
                InstrumentList.Add(new Security { Symbol = "UPRO" });
                InstrumentList.Add(new Security { Symbol = "AGQ" });
                InstrumentList.Add(new Security { Symbol = "XOP" });
                InstrumentList.Add(new Security { Symbol = "JPM" });
                InstrumentList.Add(new Security { Symbol = "QLD" });
                InstrumentList.Add(new Security { Symbol = "EWC" });
                InstrumentList.Add(new Security { Symbol = "XHB" });
                InstrumentList.Add(new Security { Symbol = "IVV" });
                InstrumentList.Add(new Security { Symbol = "JNK" });
                SearchResults = new ObservableCollection<Security>(InstrumentList);
                EditProperties=new DelegateCommand(OpenChangeProperties);
                InstrumentSelected=new DelegateCommand(SubscribeInstrument);
                ConnectToProvider=new DelegateCommand(Connect);
                EventSystem.Subscribe<Unsubscribe>(UnsubscriptionArrived);
                EventSystem.Subscribe<Provider>(SwitchProvider);
                EventSystem.Subscribe<LoginArrivedMessage>(LoginArrived);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "ProviderViewModel");
            }
        }

        /// <summary>
        /// Event Fired On Login Arrived.
        /// </summary>
        /// <param name="loginArrivedMessage"></param>
        public void LoginArrived(LoginArrivedMessage loginArrivedMessage)
        {
            
        }

        /// <summary>
        /// Event fired whenever user switches tab in Dataview
        /// </summary>
        /// <param name="provider"></param>
        public void SwitchProvider(Provider provider)
        {
            CurrentSelectedProviderTab = provider;
        }

        /// <summary>
        /// Event fired when UnsubscriptionArrives
        /// </summary>
        /// <param name="unsubscribe"></param>
        public void UnsubscriptionArrived(Unsubscribe unsubscribe)
        {
            InstrumentList.Add(unsubscribe.Security);
            OnSearchChanged(Search);
        }

        /// <summary>
        /// When A certain Provider sends a connection call
        /// </summary>
        private void Connect()
        {
            if (!SelectedProvider.IsConnected)
            {
                EventSystem.Publish<LogonRequestGenerated>(new LogonRequestGenerated
                    {RequestForLogIn = SelectedProvider});
                EventSystem.Publish<Login>(new Login {MarketDataProvider = SelectedProvider.ProviderName});
            }
            else
            {
                EventSystem.Publish<Logout>(new Logout{MarketDataProvider = SelectedProvider.ProviderName});
            }
        }

        /// <summary>
        /// Command To Subscribe Instrument
        /// </summary>
        private void SubscribeInstrument()
        {
            try
            {
                EventSystem.Publish<SecurityPermissions>(new SecurityPermissions
                    {
                        Id = Guid.NewGuid().ToString(),
                        MarketDataProvider = CurrentSelectedProviderTab.ProviderName,
                        Security = SelectedInstrument,
                    });
                //InstrumentList.Remove(SelectedInstrument);
                //SearchResults.Remove(SelectedInstrument);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "SubscribeInstrument");
            }
        }

        /// <summary>
        /// This command opens the Change 
        /// properties window of a certain broker 
        /// </summary>
        private void OpenChangeProperties()
        {
            try
            {
                _writePermissionView.ShowDialog();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "ChangeProperties");
            }
        }

        /// <summary>
        /// Methord Used to get search List.
        /// </summary>
        /// <param name="searchString"></param>
        private void OnSearchChanged(string searchString)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchString))
                {
                    //Add Elements of InstrumentList to SearchResults When Search String is Empty. 
                    SearchResults.Clear();
                    foreach (var security in InstrumentList)
                    {
                        SearchResults.Add(security);
                    }
                }
                else
                {
                    //Filter InstrumentList according to the Search string and Adds it to SearchResults 
                    SearchResults.Clear();
                    var temp = new ObservableCollection<Security>(InstrumentList.Where(d => d.Symbol.Contains(searchString)));
                    foreach (var security in temp)
                    {
                        SearchResults.Add(security);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "OnSearchChanged");
            }
        }
    }
}
