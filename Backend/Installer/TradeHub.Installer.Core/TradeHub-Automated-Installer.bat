heat dir "C:\Code Repo\Trade Hub\MarketDataEngine\TradeHub.MarketDataEngine.Server.WindowsService\bin\Release" -dr TradeHub.MDE -cg TradeHub.MDE -gg -g1 -sf -srd -sreg -var "var.MyDir" -out "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.Core\Fragments\MDE.wxs"

heat dir "C:\Code Repo\Trade Hub\DataDownloader\TradeHub.DataDownloader.UserInterface\bin\Release" -dr TradeHub.DD -cg TradeHub.DD -gg -g1 -sf -srd -sreg -var "var.DDDir" -out "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.Core\Fragments\DD.wxs"

heat dir "C:\Code Repo\Trade Hub\OrderExecutionEngine\TradeHub.OrderExecutionEngine.Server.WindowsService\bin\Release" -dr TradeHub.OEE -cg TradeHub.OEE -gg -g1 -sf -srd -sreg -var "var.OEEDir" -out "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.Core\Fragments\OEE.wxs"

heat dir "C:\TradeHub 06-08-2013\StockTrader" -dr StockTrader -cg StockTrader -gg -g1 -sf -srd -sreg -var "var.StockDir" -out "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.Core\Fragments\StockTrader.wxs"

heat dir "C:\Code Repo\Trade Hub\StrategyRunner\UserInterfaceLayer\TradeHub.StrategyRunner.UserInterface\bin\Release" -dr TradeHub.SR -cg TradeHub.SR -gg -g1 -sf -srd -sreg -var "var.StrategyDir" -out "C:\Code Repo\Trade Hub\Installer\TradeHub.Installer.Core\Fragments\StrategyRunner.wxs"