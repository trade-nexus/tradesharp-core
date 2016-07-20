using System;
using Microsoft.Win32;

namespace TradeHub.StrategyRunner.UserInterface.SearchModule.Utility
{
    public class FileDialogService
    {
        public string FileName
        {
            get { return _fileName; }
        }

        public Nullable<bool> OpenFileDialog(string extension, string fileInfo)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            // Set filter for file extension and default file extension
            fileDialog.DefaultExt = extension;
            fileDialog.Filter = fileInfo + "(" + extension + ")|*" + extension;

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = fileDialog.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Get file name
                this._fileName = fileDialog.FileName;
            }

            return result;
        }

        private string _fileName;
    }
}
