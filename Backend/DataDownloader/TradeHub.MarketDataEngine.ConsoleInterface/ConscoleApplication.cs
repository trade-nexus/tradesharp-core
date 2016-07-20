using System;
using Spring.Context;
using Spring.Context.Support;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.DataDownloader.ApplicationCenter;

namespace TradeHub.MarketDataEngine.ConsoleInterface
{
    /// <summary>
    /// THIS CLASS IS ONLY FOR TESTING PURPOSE
    /// This is not a permanent Class. It will be replaced by our UI Layer. 
    /// </summary>
    public class ConscoleApplication
    {
        public static ApplicationControl ApplicationControl;
        static void Main()
        {
            IApplicationContext context = ContextRegistry.GetContext();
            ApplicationControl = (ApplicationControl) context.GetObject("ApplicationControl");
            ApplicationControl.OnDataArrived+=ApplicationControlOnDataArrived;
            ApplicationControl.OnLogonArrived+=ApplicationControlOnLogonArrived;
            ApplicationControl.OnLogoutArrived+=ApplicationControlOnLogOutArrived;
            ApplicationControl.SendLogonRequest(new Login { MarketDataProvider = MarketDataProvider.Blackwood });

            Console.ReadLine();
            ApplicationControl.Logout(new Logout { MarketDataProvider = MarketDataProvider.Blackwood });
        }

        private static void ApplicationControlOnLogOutArrived(string newdata)
        {
            Console.WriteLine(newdata);
        }

        private static void ApplicationControlOnLogonArrived(string newdata)
        {
            

            Console.WriteLine(newdata);
        }

        private static void ApplicationControlOnDataArrived(MarketDataEvent newdata)
        {
            Console.WriteLine(newdata);
        }
    }
}
