using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using TradeHub.StrategyRunner.UserInterface.Common;
using TradeHub.StrategyRunner.UserInterface.SettingsModule.Utility;

namespace TradeHub.StrategyRunner.UserInterface.SettingsModule.ViewModel
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        private string _path = @"HistoricalDataConfiguration\HistoricalDataProvider.xml";

        /// <summary>
        /// Start Date for the Historical Data to be used
        /// </summary>
        private string _startDate;

        /// <summary>
        /// End Date for the Historical Data to be used
        /// </summary>
        private string _stopDate;

        /// <summary>
        /// Start Date for the Historical Data to be used
        /// </summary>
        public string StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                RaisePropertyChanged("StartDate");
            }
        }

        /// <summary>
        /// End Date for the Historical Data to be used
        /// </summary>
        public string StopDate
        {
            get { return _stopDate; }
            set
            {
                _stopDate = value;
                RaisePropertyChanged("StopDate");
            }
        }

        public ICommand SaveSettingsCommand { get; set; }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SettingsWindowViewModel()
        {
            SaveSettingsCommand = new DelegateCommand(SaveSettings);

            EventSystem.Subscribe<string>(UpdateCurrentValues);
        }

        /// <summary>
        /// Saves current values for Start/Stop dates
        /// </summary>
        private void SaveSettings()
        {
            if(!VerifyValues())
            {
                return;
            }

            UpdateSettingsFile();

            EventSystem.Publish<string>("CloseSettingsWindow");
        }

        /// <summary>
        /// Verifies if the correct values are present
        /// </summary>
        /// <returns></returns>
        private bool VerifyValues()
        {
            // Check Start Date value
            var tempArray = _startDate.Split(',');
            if(tempArray.Length!=3)
            {
                return false;
            }

            // Check Stop Date value
            tempArray = _stopDate.Split(',');
            if (tempArray.Length != 3)
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Updates the current values according to the saved data in settings file
        /// </summary>
        /// <param name="value"></param>
        private void UpdateCurrentValues(string value)
        {
            // Get Current Directory
            var directory = System.AppDomain.CurrentDomain.BaseDirectory;

            if (value.Equals("UpdateSettingsValues"))
            {
                var values = XmlFileHandler.GetValues(directory + @"\" + _path);

                _startDate = values.Item1;
                _stopDate = values.Item2;
            }
        }

        /// <summary>
        /// Updates values in the settings file 
        /// </summary>
        private void UpdateSettingsFile()
        {
            // Get Current Directory
            var directory = System.AppDomain.CurrentDomain.BaseDirectory;

            XmlFileHandler.SaveValues(_startDate, _stopDate, directory + @"\" + _path);
        }
    }
}
