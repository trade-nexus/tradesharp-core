using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TradeSharpLicense.Manager
{
    internal class BinaryFileReader
    {
        /// <summary>
        /// Read Data from file
        /// </summary>
        public string Read()
        {
            try
            {
                using (var binaryReader = new BinaryReader(File.Open("TradeSharpLicense.obj", FileMode.Open)))
                {
                    var byteBuffer = new byte[144];

                    for (int i = 0; i < 144; i++)
                    {
                        byteBuffer[i] = FromHex(binaryReader.ReadString()).First();   
                    }

                    var stringData = Encoding.ASCII.GetString(byteBuffer);
                    return stringData;
                }
            }
            catch (Exception exception)
            {
                throw;
            }

            return String.Empty;
        }

        /// <summary>
        /// Read Data from file
        /// </summary>
        public byte[] ReadBytes()
        {
            try
            {
                using (var binaryReader = new BinaryReader(File.Open("TradeSharpLicense.obj", FileMode.Open)))
                {
                    var byteBuffer = new byte[144];

                    for (int i = 0; i < 144; i++)
                    {
                        byteBuffer[i] = FromHex(binaryReader.ReadString()).First();
                    }

                    return byteBuffer;
                }
            }
            catch (Exception exception)
            {
                throw;
            }

            return null;
        }

        public static byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }
    }
}
