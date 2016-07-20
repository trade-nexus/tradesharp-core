using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TradeHub.UserInterface.ComponentsModule.ViewModels;

namespace TradeHub.UserInterface.ComponentsModule.Commands
{
    class OpenDataDownloaderCommand:ICommand
    {
        private ComponentsViewModel _viewModel;
        public OpenDataDownloaderCommand(ComponentsViewModel viewModel)
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
            _viewModel.OpenMdd();
        }
    
    }
}
