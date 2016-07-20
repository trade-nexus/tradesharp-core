using System;
using System.Windows.Input;
using TradeHub.UserInterface.ServicesModule.ViewModel;

namespace TradeHub.UserInterface.ServicesModule.Commands
{
    class RestartMdeCommand:ICommand
    {
        private TradeHubServicesViewModel _viewModel;

        public RestartMdeCommand(TradeHubServicesViewModel viewModel)
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
            _viewModel.RestartMdeService();
        }
    }
}
