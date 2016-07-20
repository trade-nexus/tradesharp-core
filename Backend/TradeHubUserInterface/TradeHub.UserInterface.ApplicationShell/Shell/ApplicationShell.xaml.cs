using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using TradeHub.UserInterface.Common;

namespace TradeHub.UserInterface.ApplicationShell.Shell
{
    /// <summary>
    /// Interaction logic for ApplicationShell.xaml
    /// </summary>
    public partial class ApplicationShell : Window
    {
        public ApplicationShell()
        {
            InitializeComponent();
        }

        private void ApplicationShell_OnClosing(object sender, CancelEventArgs e)
        {
            MessageBoxResult result=MessageBox.Show("Are you sure you want to exit, all the strategies and services running will be stopped?",
                "Confiramtion", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                EventSystem.Publish<string>("ShutDown");
                Logger.Info("Closing Application", typeof(ApplicationShell).FullName, "ApplicationShell_OnClosing");
                
           }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
