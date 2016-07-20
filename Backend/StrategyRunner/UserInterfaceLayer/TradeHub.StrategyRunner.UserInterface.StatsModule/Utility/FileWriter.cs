using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeHub.Common.Core.DomainModels.OrderDomain;

namespace TradeHub.StrategyRunner.UserInterface.StatsModule.Utility
{
    public class FileWriter
    {
        /// <summary>
        /// Writes order executions in CSV File
        /// </summary>
        public static void WriteFile(string path, ObservableCollection<Execution> statsCollection)
        {
            string activeDir = path;
            string newPath = Path.Combine(activeDir, string.Format("DATA_{0:yyyy-MM-dd}", DateTime.Now));
            Directory.CreateDirectory(newPath);
            string newFileName = string.Empty;
            newFileName = string.Format("stats_{0:hh-mm-ss-tt}.txt", DateTime.Now);
            string newLine = Environment.NewLine;
            newPath = Path.Combine(newPath, newFileName);

            if (!File.Exists(newPath))
            {
                StreamWriter outputFile = new StreamWriter(newPath);
                foreach (Execution execution in statsCollection)
                {
                    outputFile.WriteLine(execution.BasicExecutionInfo());
                }
                outputFile.Close();
            }
        }
    }
}
