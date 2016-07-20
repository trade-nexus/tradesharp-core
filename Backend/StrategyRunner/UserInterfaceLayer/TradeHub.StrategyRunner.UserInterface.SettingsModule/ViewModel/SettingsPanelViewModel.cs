using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Spring.Context.Support;
using TradeHub.StrategyRunner.UserInterface.Common;
using TradeHub.StrategyRunner.UserInterface.SettingsModule.View;

namespace TradeHub.StrategyRunner.UserInterface.SettingsModule.ViewModel
{
    public class SettingsPanelViewModel : ViewModelBase
    {
        /// <summary>
        /// Command to edit Start/Stop times for Historical Data
        /// </summary>
        public ICommand EditHistoricalDataConfig { get; set; }

        private SettingsWindow _settingsWindow;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public SettingsPanelViewModel()
        {
            EditHistoricalDataConfig = new DelegateCommand(EditHistoricalDataSettings);

            EventSystem.Subscribe<string>(OnSaveSettings);
        }

        /// <summary>
        /// Called when event is raised to edit Historical Data settings
        /// </summary>
        private void EditHistoricalDataSettings()
        {
            // Get View to display details
            var context = ContextRegistry.GetContext();
            _settingsWindow = context.GetObject("SettingsWindowView") as SettingsWindow;

            EventSystem.Publish<string>("UpdateSettingsValues");

            _settingsWindow.ShowDialog();
        }

        private void OnSaveSettings(string value)
        {
            if (value.Equals("CloseSettingsWindow"))
            {
                _settingsWindow.Hide();
            }
        }
    }
}
