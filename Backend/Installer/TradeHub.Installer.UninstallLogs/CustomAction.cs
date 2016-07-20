using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;

namespace TradeHub.Installer.UninstallLogs
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult RemoveLogs(Session session)
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                              "\\TradeSharp Logs";
                session.Log("Begin Removing Log Files Path=" + path);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    session.Log("Removed Log Files from Path=" + path);
                }
                else
                {
                    session.Log("Directory does not exists");
                }
                
            }
            catch (Exception exception)
            {
                session.Log("Exception occured, Exception Message="+exception.Message);
                return ActionResult.Failure;
            }
            return ActionResult.Success;
        }
    }
}
