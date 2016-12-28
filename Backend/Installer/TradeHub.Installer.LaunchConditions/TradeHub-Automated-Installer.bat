heat dir "C:\TradeSharp\MarketDataEngine" -dr TradeHub.MDE -cg TradeHub.MDE -gg -g1 -sf -srd -sreg -var "var.MyDir" -out "C:\trade-nexus\tradesharp-core\Backend\Installer\TradeHub.Installer.Core\Fragments\MDE.wxs"

heat dir "C:\TradeSharp\OrderExecutionEngine" -dr TradeHub.OEE -cg TradeHub.OEE -gg -g1 -sf -srd -sreg -var "var.OEEDir" -out "C:\trade-nexus\tradesharp-core\Backend\Installer\TradeHub.Installer.Core\Fragments\OEE.wxs"

heat dir "C:\TradeSharp\PositionEngine" -dr TradeHub.PE -cg TradeHub.PE -gg -g1 -sf -srd -sreg -var "var.PEDir" -out "C:\trade-nexus\tradesharp-core\Backend\Installer\TradeHub.Installer.Core\Fragments\PE.wxs"

heat dir "C:\TradeSharp\TradeManager" -dr TradeHub.TM -cg TradeHub.TM -gg -g1 -sf -srd -sreg -var "var.TMDir" -out "C:\trade-nexus\tradesharp-core\Backend\Installer\TradeHub.Installer.Core\Fragments\TM.wxs"

heat dir "C:\TradeSharp\TradeHubGui" -dr TradeHub.UI -cg TradeHub.UI -gg -g1 -sf -srd -sreg -var "var.UIDir" -out "C:\trade-nexus\tradesharp-core\Backend\Installer\TradeHub.Installer.Core\Fragments\UI.wxs"

heat dir "C:\TradeSharp\Configuration" -dr TradeHub.IS -cg TradeHub.IS -gg -g1 -sf -srd -sreg -var "var.ISDir" -out "C:\trade-nexus\tradesharp-core\Backend\Installer\TradeHub.Installer.Core\Fragments\IS.wxs"

heat dir "C:\TradeSharp\TemplateInstaller" -dr TradeHub.TS -cg TradeHub.TS -gg -g1 -sf -srd -sreg -var "var.TSDir" -out "C:\trade-nexus\tradesharp-core\Backend\Installer\TradeHub.Installer.Core\Fragments\TS.wxs"