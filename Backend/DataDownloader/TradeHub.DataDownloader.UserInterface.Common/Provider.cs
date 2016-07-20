
namespace TradeHub.DataDownloader.UserInterface.Common
{
    /// <summary>
    /// This Class will be Shifted To Core
    /// </summary>
    public class Provider:ViewModelBase
    {
        /// <summary>
        /// Saves the name of the provider.
        /// </summary>
        public string ProviderName { get; set; }

        /// <summary>
        /// Status Of provider.
        /// </summary>
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged("IsConnected");
                MenuText = value ? "Disconnect" : "Connect";
            }
        }
        
        private string _menuText="Connect";
        public string MenuText
        {
            get { return _menuText; }
            private set { _menuText = value; RaisePropertyChanged("MenuText"); }
        }

    }
}
