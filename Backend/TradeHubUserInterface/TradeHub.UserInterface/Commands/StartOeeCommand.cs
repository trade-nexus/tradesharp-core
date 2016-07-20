using System;
using System.Windows.Input;
using TradeHub.UserInterface.ServicesModule.ViewModel;

namespace TradeHub.UserInterface.ServicesModule.Commands
{
    public class StartOeeCommand:ICommand
    {
        private TradeHubServicesViewModel _viewModel;

        public StartOeeCommand(TradeHubServicesViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _viewModel.StartOeeService();
        }
    }
}
