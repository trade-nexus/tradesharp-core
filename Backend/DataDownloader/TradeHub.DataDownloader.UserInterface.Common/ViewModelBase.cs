using System.ComponentModel;

namespace TradeHub.DataDownloader.UserInterface.Common
{

    /// <summary>
    /// Base Class of Every View Model
    /// Contains Functionality which is common in every View Model
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Raise Event When ever a property is changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
