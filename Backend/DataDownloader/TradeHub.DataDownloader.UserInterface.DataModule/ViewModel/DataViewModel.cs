using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Permissions;
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

namespace TradeHub.DataDownloader.UserInterface.DataModule.ViewModel
{
    /// <summary>
    ///Provides The Backend Fucntionality to ModuleView
    /// </summary>
    public class DataViewModel:ViewModelBase
    {
        private Type _oType = typeof(DataViewModel);
        public ObservableCollection<SecurityStatisticsViewModel> CurrentSecurityList { get; set; }
        public ObservableCollection<Subscribe> SecutiryList { get; set; }
        public IDictionary<string, IList<SecurityStatisticsViewModel>> SecurityStatDictionary { get; set; }

        /// <summary>
        /// Changes When Selection Changes in List View
        /// </summary>
        private SecurityStatisticsViewModel _selectedSecurity;
        public SecurityStatisticsViewModel SelectedSecurity
        {
            get { return _selectedSecurity; }
            set
            {
                _selectedSecurity = value;
                RaisePropertyChanged("SelectedSecurity");
            }
        }

        /// <summary>
        /// Command To Unsubscribe Data
        /// </summary>
        public ICommand RemoveSecurity { get; set; }

        /// <summary>
        /// Current Selected Provider From Tab Menu
        /// </summary>
        private Provider _selectedProvider;
        public Provider SelectedProvider
        {
            get { return _selectedProvider; }
            set
            {
                try
                {
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Selected provider changed" + value.ProviderName, _oType.FullName, "SelectedProvider");
                    }
                    _selectedProvider = value;
                    ReloadList(value);
                    EventSystem.Publish<Provider>(_selectedProvider);
                }
                catch (Exception exception)
                {
                    Logger.Error(exception, _oType.FullName, "SelectedProvider");
                }
            }
        }

        /// <summary>
        /// Methord Called when provider tab is changed
        /// Or New Symbol is Added
        /// </summary>
        /// <param name="value">Takes The Name of The provider</param>
        private void ReloadList(Provider value)
        {
            try
            {
                CurrentSecurityList.Clear();
                foreach (var security in SecurityStatDictionary[value.ProviderName])
                {
                    CurrentSecurityList.Add(security);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "ProviderChanged");
            }
        }

        /// <summary>
        /// Collection To Populate TabView Headers
        /// </summary>
        private ObservableCollection<Provider> _selectedProviders;
        public ObservableCollection<Provider> SelectedProviders
        {
            get { return _selectedProviders; }
            set { _selectedProviders = value; }
        }


        public DataViewModel()
        {
            try
            {
                SelectedProviders = new ObservableCollection<Provider>();
                SecurityStatDictionary=new Dictionary<string, IList<SecurityStatisticsViewModel>>();
                SecurityStatDictionary[MarketDataProvider.Blackwood] = new List<SecurityStatisticsViewModel>();
                SecurityStatDictionary[MarketDataProvider.Simulated] = new List<SecurityStatisticsViewModel>();
                SecurityStatDictionary[MarketDataProvider.InteractiveBrokers] = new List<SecurityStatisticsViewModel>();
                CurrentSecurityList = new ObservableCollection<SecurityStatisticsViewModel>();
                SecutiryList = new ObservableCollection<Subscribe>();
                EventSystem.Subscribe<SecurityPermissions>(SubscribeToNewSymbol);
                EventSystem.Subscribe<Tick>(OnTickArrived);
                EventSystem.Subscribe<Bar>(OnBarArrived);
                EventSystem.Subscribe<LoginArrivedMessage>(LoginArrived);
                EventSystem.Subscribe<LogonRequestGenerated>(LogonRequestArrived);
                EventSystem.Subscribe<Logout>(OnLogoutArrived);
                RemoveSecurity=new DelegateCommand(UnsubscribeSecurity);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "DataViewModel");
            }
        }

        /// <summary>
        /// When User Disconnect a provider
        /// </summary>
        public void OnLogoutArrived(Logout logout)
        {
            SecurityStatDictionary[logout.MarketDataProvider].Clear();
            var providerTemp = SelectedProviders.Single(x => x.ProviderName == logout.MarketDataProvider);
            providerTemp.IsConnected = false;
            SelectedProviders.Remove(providerTemp);
        }

        /// <summary>
        /// Logon Request For Certain Provider
        /// Main Purpose of the methord is to 
        /// create New Tab for Requested Provider
        /// </summary>
        public void LogonRequestArrived(LogonRequestGenerated logonRequestGenerated)
        {
            try
            {
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info("Creating Tab For " + logonRequestGenerated.RequestForLogIn.ProviderName,
                                _oType.FullName, "LogonRequestArrived");
                }
                SelectedProviders.Add(logonRequestGenerated.RequestForLogIn);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "LogonRequestArrived");
            }
        }

        /// <summary>
        /// Event Fired On Login Arrived.
        /// </summary>
        /// <param name="loginArrivedMessage"></param>
        public void LoginArrived(LoginArrivedMessage loginArrivedMessage)
        {
            var provider = SelectedProviders.Single(x => x.ProviderName == loginArrivedMessage.Provider.ProviderName);
            provider.IsConnected = true;
        }

        /// <summary>
        /// Event Fired on every Bar
        /// </summary>
        /// <param name="bar"></param>
        public void OnBarArrived(Bar bar)
        {
            try
            {
                var list = SecurityStatDictionary[bar.MarketDataProvider];
                var temp = list.Single(x => x.Symbol == bar.Security.Symbol);
                if (temp.BarChecked)
                {
                    temp.NumberOfBars++;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "OnBarArrived");
            }
        }

        /// <summary>
        /// Event Fired when ever New Tick
        /// Increments count on UI
        /// </summary>
        /// <param name="tick"></param>
        public void OnTickArrived(Tick tick)
        {
            try
            {
                var list =SecurityStatDictionary[tick.MarketDataProvider];
                var temp = list.Single(x => x.Symbol == tick.Security.Symbol);
                if ((tick.HasAsk||tick.HasBid)&&temp.QuoteChecked)
                {
                    temp.NumberOfQuotes++;
                }
                if (tick.HasTrade && temp.TradeChecked)
                {
                    temp.NumberOfTrades++;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "OnTickArrived");
            }
        }

        /// <summary>
        /// Unsubscribe the selected security from gridview.
        /// </summary>
        private void UnsubscribeSecurity()
        {
            try
            {
                Unsubscribe unsubscribe = new Unsubscribe();
                Subscribe subscribe = SecutiryList.Single(s => s.Id == SelectedSecurity.Id);
                unsubscribe.Security = subscribe.Security;
                if (Logger.IsInfoEnabled)
                {
                    Logger.Info(unsubscribe.ToString(), _oType.FullName, "UnsubscribeSecurity");
                }
                unsubscribe.Id = subscribe.Id;

                unsubscribe.MarketDataProvider = SelectedProvider.ProviderName;
                var temp = SecurityStatDictionary[SelectedProvider.ProviderName];
                var selectedRowOfGrid = temp.Single(x => x.Id == unsubscribe.Id);
                
                EventSystem.Publish<Unsubscribe>(unsubscribe);
                EventSystem.Publish<UnsubscribeBars>(new UnsubscribeBars
                    {
                        UnSubscribeBarDataRequest = new BarDataRequest
                            {
                                Id = unsubscribe.Id,
                                MarketDataProvider = unsubscribe.MarketDataProvider,
                                Security = unsubscribe.Security,
                                BarFormat = selectedRowOfGrid.BarSettingView.BarSettingViewModel.SelectedFormate,
                                BarPriceType = selectedRowOfGrid.BarSettingView.BarSettingViewModel.SelectedType,
                                BarLength = selectedRowOfGrid.BarSettingView.BarSettingViewModel.BarLength,
                                PipSize = selectedRowOfGrid.BarSettingView.BarSettingViewModel.PipSize,
                            }
                    });
                SecutiryList.Remove(subscribe);
                SecurityStatDictionary[SelectedProvider.ProviderName].Remove(
                    SecurityStatDictionary[SelectedProvider.ProviderName].Single(x => x.Id == unsubscribe.Id));
                ReloadList(SelectedProvider);


            }
            catch (Exception exception)
            {
                Logger.Error(exception, _oType.FullName, "UnsubscribeSecurity");
            }

        }

        /// <summary>
        /// When User Subscribe to New Symbol.
        /// </summary>
        /// <param name="security"></param>
        private void SubscribeToNewSymbol(SecurityPermissions security)
        {
            if (Logger.IsInfoEnabled)
            {
                Logger.Info(security.ToString(), _oType.FullName, "SubscribeToNewSymbol");
            }
            if (!SecurityStatDictionary[SelectedProvider.ProviderName].Any(x=>x.Symbol==security.Security.Symbol))
            {
                SecutiryList.Add(security);
                SecurityStatDictionary[SelectedProvider.ProviderName].Add(new SecurityStatisticsViewModel
                    {Symbol = security.Security.Symbol, Id = security.Id, ProviderName = SelectedProvider.ProviderName});
                ReloadList(SelectedProvider);
            }
        }
    }
}
