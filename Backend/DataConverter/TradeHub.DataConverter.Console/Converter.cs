using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel;
using TraceSourceLogger;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.DataConverter.Common;
using TradeHub.DataConverter.EsignalToDataDownloader;

namespace TradeHub.DataConverter.Console
{
    public class Converter
    {
        private static Type _type = typeof (Convert);

        static void Main(string[] args)
        {
            try
            {
                String docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\eSignal Files";

                if (Directory.Exists(docPath))
                {
                    var directory = new DirectoryInfo(docPath);

                    //foreach (FileInfo file in directory.GetFiles("*.xlsx"))
                    //{
                    //    ConvertExcelToCsv(file);
                    //}

                    foreach (FileInfo file in directory.GetFiles("*.xlsx"))
                    {
                        var dataSet = ReadFromExcelFile(file);
                        if (dataSet == null) return;

                        int rowNo = 1;
                        while (rowNo < dataSet.Tables[0].Rows.Count)
                        {
                            string data = string.Empty;

                            for (int i = 0; i < dataSet.Tables[0].Columns.Count; i++)
                            {
                                data += dataSet.Tables[0].Rows[rowNo][i].ToString() + ",";
                            }

                            string dataConverted = BarConverter.ConvertBars(data, file.Name.Split('.')[0]);

                            DetailBar detailBar = new DetailBar(new Bar(""));
                            detailBar.DateTime = DateTime.ParseExact(dataConverted.Split(',')[6],
                                                                     "M/d/yyyy h:mm:ss tt",
                                                                     CultureInfo.InvariantCulture);
                            detailBar.BarFormat = BarFormat.TIME;
                            detailBar.BarPriceType = BarPriceType.LAST;
                            detailBar.BarLength = 60;

                            FileWriter.WriteData(dataConverted, file.Name.Split('.')[0], detailBar);
                            rowNo++;
                        }
                    }

                    //foreach (FileInfo file in directory.GetFiles("*.csv"))
                    //{
                    //    FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.None);
                    //    StreamReader streamReader = new StreamReader(fileStream);

                    //    string dataRead = streamReader.ReadLine();

                    //    while ((dataRead = streamReader.ReadLine()) != null)
                    //    {
                    //        string dataConverted = BarConverter.CovertBars(dataRead, file.Name);

                    //        DetailBar detailBar= new DetailBar(new Bar(""));

                    //        detailBar.BarFormat = BarFormat.TIME;
                    //        detailBar.BarPriceType = BarPriceType.LAST;
                    //        detailBar.BarLength = 62;
                        
                    //        FileWriter.WriteData(dataConverted, file.Name, detailBar);
                    //    }

                    //    streamReader.Close();
                    //    fileStream.Close();
                    //}
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "Main");
            }
        }

        /// <summary>
        /// Converts Excel file to CSV format
        /// </summary>
        /// <param name="fileInfo">Complete file info</param>
        static void ConvertExcelToCsv(FileInfo fileInfo)
        {
            try
            {
                FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.None);

                // Reading from a OpenXml Excel file (2007 format; *.xlsx)
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);

                // DataSet - The result of each spreadsheet will be created in the result.Tables
                DataSet result = excelReader.AsDataSet();

                // Free resources (IExcelDataReader is IDisposable)
                excelReader.Close();

                string csvData = "";
                int rowNo = 0;

                while (rowNo < result.Tables[0].Rows.Count)
                {
                    for (int i = 0; i < result.Tables[0].Columns.Count; i++)
                    {
                        csvData += result.Tables[0].Rows[rowNo][i].ToString() + ",";
                    }
                    rowNo++;
                    csvData += "\n";
                }

                string output = fileInfo.Directory + "\\" + fileInfo.Name.Split('.')[0] + ".csv"; // define your own filepath & filename
                StreamWriter csv = new StreamWriter(@output, false);
                csv.Write(csvData);
                csv.Close();

                fileStream.Close();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ConvertExcelToCsv");
            }
        }

        /// <summary>
        /// Converts Excel file to CSV format
        /// </summary>
        /// <param name="fileInfo">Complete file info</param>
        static DataSet ReadFromExcelFile(FileInfo fileInfo)
        {
            try
            {
                FileStream fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.None);

                // Reading from a OpenXml Excel file (2007 format; *.xlsx)
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fileStream);

                // DataSet - The result of each spreadsheet will be created in the result.Tables
                DataSet result = excelReader.AsDataSet();

                // Free resources (IExcelDataReader is IDisposable)
                excelReader.Close();

                return result;
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "ReadFromExcelFile");
                return null;
            }
        }
    }
}
