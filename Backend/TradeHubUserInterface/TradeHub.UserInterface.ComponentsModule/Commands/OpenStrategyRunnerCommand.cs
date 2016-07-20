using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TradeHub.UserInterface.ComponentsModule.ViewModels;

namespace TradeHub.UserInterface.ComponentsModule.Commands
{
    class OpenStrategyRunnerCommand:ICommand
    {
        private ComponentsViewModel _viewModel;
        public OpenStrategyRunnerCommand(ComponentsViewModel viewModel)
        {
            _viewModel = viewModel;
        }
        public bool CanExecute(object parameter)
        {
            return File.Exists(@"..\Strategy Runner\TradeHub.StrategyRunner.UserInterface.exe");
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _viewModel.OpenStrategyRunner();
        }
    }
}
