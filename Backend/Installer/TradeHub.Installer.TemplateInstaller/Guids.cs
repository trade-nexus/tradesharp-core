// Guids.cs
// MUST match guids.h
using System;

namespace AuroraSolutions.TradeHub_Installer_TemplateInstaller
{
    static class GuidList
    {
        public const string guidTradeHub_Installer_TemplateInstallerPkgString = "a497c5a6-5e1e-4725-9710-77c551bd53fd";
        public const string guidTradeHub_Installer_TemplateInstallerCmdSetString = "6cb8a4ff-41ba-423a-b557-357994505c10";

        public static readonly Guid guidTradeHub_Installer_TemplateInstallerCmdSet = new Guid(guidTradeHub_Installer_TemplateInstallerCmdSetString);
    };
}