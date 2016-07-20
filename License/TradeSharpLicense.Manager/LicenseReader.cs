using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeSharpLicense.Manager
{
    public class LicenseReader
    {
        /// <summary>
        /// Reads the available License file and extracts relevant information
        /// </summary>
        /// <returns></returns>
        public string Read()
        {
            return GetLicenseInformation().ToString();
        }

        private Tuple<LicenseType, string, DateTime> GetLicenseInformation()
        {
            Decryptor decryptor = new Decryptor();
            var licenseInformation = decryptor.DecryptLicense(ReadLicenseFile());

            return new Tuple<LicenseType, string, DateTime>(
                FindLicenseType(licenseInformation.Item1),
                licenseInformation.Item2.Trim(),
                ExtracetExpirationDate(licenseInformation.Item3));
        }

        private byte[] ReadLicenseFile()
        {
            BinaryFileReader binaryFileReader = new BinaryFileReader();

            return binaryFileReader.ReadBytes();
        }

        private LicenseType FindLicenseType(string licenseType)
        {
            switch (licenseType.Trim())
            {
                case "monthly":
                    return LicenseType.Monthly;
                case "annual":
                    return LicenseType.Annual;
                case "lifetime":
                    return LicenseType.LifeTime;
                default:
                    return LicenseType.Demo;
            }
        }

        private DateTime ExtracetExpirationDate(string date)
        {
            return DateTime.ParseExact(date.Trim(), "yyyyMMdd", CultureInfo.InvariantCulture);
        }
    }
}
