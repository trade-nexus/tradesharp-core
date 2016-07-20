
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.DataDownloader.UserInterface.Common;

namespace TradeHub.DataDownloader.UserInterface.DataModule.ViewModel
{
    public class BarSettingViewModel:ViewModelBase
    {
        public ObservableCollection<string> BarTypes { get; set; }
        public ObservableCollection<string> BarFormates { get; set; }
        public DataViewModel DataViewModel { get; set; }
        private Type _oType = typeof(BarSettingViewModel);

        public SecurityStatisticsViewModel StatisticsViewModel { get; set; }

        /// <summary>
        /// Enable or Disable Controls 
        /// </summary>
        private bool _isControlsEnabled=true;
        public bool IsControlsEnabled
        {
            get { return _isControlsEnabled; }
            set
            {
                _isControlsEnabled = value;
                RaisePropertyChanged("IsControlsEnabled");
            }
        }

        /// <summary>
        /// Current Selected Type
        /// </summary>
        private string _selectedType = BarPriceType.LAST;
        public string SelectedType
        {
            get { return _selectedType; }
            set
            {
                _selectedType = value;
                RaisePropertyChanged("SelectedType");
            }
        }

        /// <summary>
        /// Current Selected Type
        /// </summary>
        private string _selectedFormate = BarFormat.TIME;
        public string SelectedFormate
        {
            get { return _selectedFormate; }
            set
            {
                _selectedFormate = value;
                RaisePropertyChanged("SelectedFormate");
            }
        }

        /// <summary>
        /// Saves The Setting of 
        /// </summary>
        public ICommand SaveSettings { get; set; } 

        /// <summary>
        /// Length Of Bar
        /// </summary>
        private decimal _barLength;
        public decimal BarLength
        {
            get { return _barLength; }
            set
            {
                _barLength = value;
                RaisePropertyChanged("BarLength");
            }
        }

        /// <summary>
        /// Pip Size
        /// </summary>
        private decimal _pipSize;
        public decimal PipSize
        {
            get { return _pipSize; }
            set
            {
                _pipSize = value;
                RaisePropertyChanged("BarLength");
            }
        }

        
        public BarSettingViewModel()
        {
            BarTypes=new ObservableCollection<string>();
            BarTypes.Add(BarPriceType.ASK);
            BarTypes.Add(BarPriceType.BID);
            BarTypes.Add(BarPriceType.LAST);
            BarTypes.Add(BarPriceType.MEAN);
            BarFormates = new ObservableCollection<string>();
            BarFormates.Add(BarFormat.DISPLACEMENT);
            BarFormates.Add(BarFormat.EQUAL_ENGINEERED);
            BarFormates.Add(BarFormat.TIME);
            BarFormates.Add(BarFormat.UNEQUAL_ENGINEERED);
            SaveSettings=new DelegateCommand(SaveCurrentSettingForBar);
        }

        /// <summary>
        /// Methord Fired When User Click On Save Command.
        /// </summary>
        private void SaveCurrentSettingForBar()
        {
            try
            {
                if (IsControlsEnabled)
                {
                    BarDataRequest newBarDataRequest = new BarDataRequest
                        {
                            BarFormat = SelectedFormate,
                            Security = new Security {Symbol = StatisticsViewModel.Symbol},
                            Id = StatisticsViewModel.Id,
                            MarketDataProvider = StatisticsViewModel.ProviderName,
                            BarLength = BarLength,
                            BarPriceType = SelectedType,
                            BarSeed = 0,
                            PipSize = PipSize
                        };
                    if (Logger.IsInfoEnabled)
                    {
                        Logger.Info("Creating Bar Request Object" + newBarDataRequest, _oType.FullName,
                                                      "SaveCurrentSettingForBar");
                    }

                    EventSystem.Publish<BarDataRequest>(newBarDataRequest);
                    IsControlsEnabled = false;
                }
            }
            catch (Exception exception)
            {
                TraceSourceLogger.Logger.Error(exception, _oType.FullName, "SaveCurrentSettingForBar");
            }
        }
    }
}
