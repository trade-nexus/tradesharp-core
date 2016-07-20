using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TradeSharpLicense.Manager
{
    internal class BinaryFileWriter
    {
        /// <summary>
        /// Write Data to file
        /// </summary>
        public void Write(string dataToWrite)
        {
            try
            {
                using (var binaryWriter = new BinaryWriter(File.Open("TradeSharpLicense.obj", FileMode.Create)))
                {
                    byte[] byteBuffer = Encoding.ASCII.GetBytes(dataToWrite);

                    foreach (var byteValue in byteBuffer)
                    {
                        binaryWriter.Write(String.Format("{0:X}",byteValue));
                    }
                    binaryWriter.Close();
                }
            }
            catch (Exception exception)
            {
                throw;
            }
        }
    }
}
