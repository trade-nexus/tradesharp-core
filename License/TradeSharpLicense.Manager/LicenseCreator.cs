using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeSharpLicense.Manager
{
    public class LicenseCreator
    {
        /// <summary>
        /// Creates a new License file with the given information
        /// </summary>
        /// <param name="expirationDate">Data at which the license should exprice. Format: YYYYMMDD</param>
        /// <param name="clientDetails">Brief client details e.g. name or company, can be max 20 characters</param>
        /// <param name="licenseType">LicenseType from the supported enums</param>
        public string Create(string expirationDate, string clientDetails, LicenseType licenseType)
        {
            // Add padding to create a 10 char long string
            expirationDate = expirationDate.PadLeft(10);

            // Add padding to complete the 20 char length
            clientDetails = clientDetails.PadRight(20);

            // Convert enum to string and add padding to fill 10 char limit
            string licenseTypeString = (licenseType.ToString()).PadLeft(10);

            // Combine all the information in a single block
            string sampleData = @expirationDate + clientDetails + licenseTypeString;

            // Initialze enrcyptor
            Encryptor encryptor = new Encryptor();

            // Get the encrypted string
            string encryptedString = encryptor.EncryptToString(sampleData);

            // Write encrypted string to create license file
            WriteBinaryData(encryptedString);

            return sampleData;
        }

        /// <summary>
        /// Writes the incoming string data in a binary file
        /// </summary>
        /// <param name="dataToWrite"></param>
        private static void WriteBinaryData(string dataToWrite)
        {
            BinaryFileWriter binaryFileWriter = new BinaryFileWriter();

            binaryFileWriter.Write(dataToWrite);
        }
    }
}
