heat dir "C:\Code Repo\Trade Hub\MarketDataEngine\TradeHub.MarketDataEngine.Server.WindowsService\bin\Release" -dr TradeHub.MDE -cg TradeHub.MDE -gg -g1 -sf -srd -sreg -var "var.MyDir" -out "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.Core\Fragments\MDE.wxs"

heat dir "C:\Code Repo\Trade Hub\OrderExecutionEngine\TradeHub.OrderExecutionEngine.Server.WindowsService\bin\Release" -dr TradeHub.OEE -cg TradeHub.OEE -gg -g1 -sf -srd -sreg -var "var.OEEDir" -out "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.Core\Fragments\OEE.wxs"

heat dir "C:\Code Repo\Trade Hub\PositionEngine\TradeHub.PositionEngine.Service\bin\Release" -dr TradeHub.PE -cg TradeHub.PE -gg -g1 -sf -srd -sreg -var "var.PEDir" -out "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.Core\Fragments\PE.wxs"

heat dir "C:\Code Repo\Trade Hub\TradeManager\TradeHub.TradeManager.Server.WindowsService\bin\Release" -dr TradeHub.TM -cg TradeHub.TM -gg -g1 -sf -srd -sreg -var "var.TMDir" -out "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.Core\Fragments\TM.wxs"

heat dir "C:\Code Repo\tradehub-ui\TradeHubGui\bin\Release" -dr TradeHub.UI -cg TradeHub.UI -gg -g1 -sf -srd -sreg -var "var.UIDir" -out "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.Core\Fragments\UI.wxs"

heat dir "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.Configuration\bin\Release" -dr TradeHub.IS -cg TradeHub.IS -gg -g1 -sf -srd -sreg -var "var.ISDir" -out "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.Core\Fragments\IS.wxs"

heat dir "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.TemplateInstaller\bin\Release" -dr TradeHub.TS -cg TradeHub.TS -gg -g1 -sf -srd -sreg -var "var.TSDir" -out "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.Core\Fragments\TS.wxs"