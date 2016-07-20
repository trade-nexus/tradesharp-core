using System;
using System.Windows.Forms;

namespace TradeHub.StrategyRunner.UserInterface.StatsModule.Utility
{
    public class FolderDialogService
    {
        public string FolderName
        {
            get { return _folderName; }
        }

        public Nullable<bool> OpenFolderDialog()
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();

            // Display OpenFileDialog by calling ShowDialog method
            DialogResult result = dlg.ShowDialog();

            // Get the selected folder name
            if (result == DialogResult.OK)
            {
                // Get file name
                this._folderName = dlg.SelectedPath;
                return true;
            }

            return false;
        }

        private string _folderName;
    }
}
