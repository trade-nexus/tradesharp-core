using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeSharpLicense.Manager;

namespace TradeSharpLicense.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.WriteLine("Starting Application");
            System.Console.WriteLine();

            // Will create the License file
            LicenseCreator licenseCreator = new LicenseCreator();

            // Will read the existing license file for details
            LicenseReader licenseReader = new LicenseReader();

            // Specify license details
            string expirationDate = "20160901";
            string clientDetails = "Peter&Ted Co.";
            LicenseType licenseType = LicenseType.Demo;

            // Create new License file
            var dataString = licenseCreator.Create(expirationDate, clientDetails, licenseType);

            System.Console.Write("Data string created: ");
            System.Console.WriteLine(dataString);
            System.Console.WriteLine();

            // Read the data in the newly created license file
            var licenseData = licenseReader.Read();

            System.Console.Write("Data read: ");
            System.Console.WriteLine(licenseData);

            System.Console.WriteLine();
            System.Console.Write("Press ANY KEY to terminate");
            System.Console.ReadLine();
        }
    }
}
