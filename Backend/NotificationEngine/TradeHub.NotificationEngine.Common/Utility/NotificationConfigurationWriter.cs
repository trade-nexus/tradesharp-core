using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TraceSourceLogger;

namespace TradeHub.NotificationEngine.Common.Utility
{
    public class NotificationConfigurationWriter
    {
        private static Type _type = typeof (NotificationConfigurationWriter);

        /// <summary>
        /// Writes email parameters to the configuration file
        /// </summary>
        /// <param name="path">configuration file path</param>
        /// <param name="parameters">email parameters i,e. Item1=username, Item2=Password</param>
        public static void WriteEmailConfiguration(string path, Tuple<string, string> parameters)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNode root = doc.DocumentElement;

                if (root != null)
                {
                    // Save Username
                    XmlNode usernameNode = root.SelectSingleNode("descendant::username");
                    if (usernameNode != null)
                    {
                        usernameNode.InnerText = parameters.Item1;
                    }

                    // Save Password
                    XmlNode passwordNode = root.SelectSingleNode("descendant::password");
                    if (passwordNode != null)
                    {
                        passwordNode.InnerText = parameters.Item2;
                    }

                    doc.Save(path);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception, _type.FullName, "WriteEmailConfiguration");
            }
        }
    }
}
