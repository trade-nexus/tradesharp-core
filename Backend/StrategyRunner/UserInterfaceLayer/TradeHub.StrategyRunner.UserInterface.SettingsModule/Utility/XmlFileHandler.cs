using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TraceSourceLogger;

namespace TradeHub.StrategyRunner.UserInterface.SettingsModule.Utility
{
    /// <summary>
    /// Provides functionality to Read/Modify given XML files
    /// </summary>
    public static class XmlFileHandler
    {
        private static Type _type = typeof (XmlFileHandler);

        /// <summary>
        /// Returns required values from the given file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Tuple<string,string> GetValues(string path)
        {
            try
            {
                string startDate = "";
                string endDate = "";

                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNode root = doc.DocumentElement;

                XmlNode startNode = root.SelectSingleNode("descendant::StartDate");

                if (startNode != null)
                {
                    startDate = startNode.InnerText;
                }

                XmlNode endNode = root.SelectSingleNode("descendant::EndDate");

                if (endNode != null)
                {
                    endDate = endNode.InnerText;
                }

                return new Tuple<string, string>(startDate, endDate);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "GetValues");
                return new Tuple<string, string>("", "");
            }
        }

        /// <summary>
        /// Saves values in the given file
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="path"></param>
        public static void SaveValues(string startDate, string endDate, string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNode root = doc.DocumentElement;

                XmlNode startNode = root.SelectSingleNode("descendant::StartDate");

                if (startNode != null)
                {
                    startNode.InnerText = startDate;
                }

                XmlNode endNode = root.SelectSingleNode("descendant::EndDate");

                if (endNode != null)
                {
                    endNode.InnerText = endDate;
                }

                doc.Save(path);
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "SaveValues");
            }
        }
    }
}
