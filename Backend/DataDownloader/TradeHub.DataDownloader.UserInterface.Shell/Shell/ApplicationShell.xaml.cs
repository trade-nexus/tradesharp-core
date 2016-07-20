using System.ComponentModel;
using System.Windows;
using TradeHub.DataDownloader.UserInterface.Common;

namespace TradeHub.DataDownloader.UserInterface.Shell.Shell
{
    /// <summary>
    /// Interaction logic for ApplicationShell.xaml
    /// </summary>
    public partial class ApplicationShell : Window
    {
        private ConfirmationMessageWindow _confirmationMessage;

        public ApplicationShell()
        {
            InitializeComponent();

            _confirmationMessage = new ConfirmationMessageWindow();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //Set Message Window Content
            _confirmationMessage.MessageBoxContent = "Are you sure you want to CLOSE application";

            //Show Message Window
            _confirmationMessage.ShowDialog();

            //Free Resources if User Confirms to Close
            if (_confirmationMessage.Selection)
            {
                base.OnClosing(e);
                EventSystem.Publish<string>("Close");
                Application.Current.Shutdown();
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
