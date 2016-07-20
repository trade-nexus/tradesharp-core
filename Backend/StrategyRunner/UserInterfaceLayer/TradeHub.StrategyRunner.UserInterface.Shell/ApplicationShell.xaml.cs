using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TraceSourceLogger;
using TradeHub.StrategyRunner.UserInterface.Common;

namespace TradeHub.StrategyRunner.UserInterface.Shell
{
    /// <summary>
    /// Interaction logic for ApplicationShell.xaml
    /// </summary>
    public partial class ApplicationShell : Window
    {
        public ApplicationShell()
        {
            InitializeComponent();
            Logger.SetLoggingLevel();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            EventSystem.Publish<string>("Close");
            Application.Current.Shutdown(); 

            // NOTE: Needs to be reviewed
            //Environment.Exit(0);
            Process.GetCurrentProcess().Kill();
        }

    }
}
