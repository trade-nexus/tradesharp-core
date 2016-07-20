using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spring.Context.Support;
using TraceSourceLogger;
using TradeHub.Common.Core.DomainModels;
using TradeHub.Common.Core.ValueObjects.AdminMessages;
using TradeHub.Common.Core.ValueObjects.MarketData;
using TradeHub.MarketDataEngine.Client.Service;

namespace TradeHub.MarketDataEngine.Client.Console
{
    class Client
    {
        private MarketDataEngineClient _marketDataEngineClient;
        private Type _type = typeof (Client);

        private int count = 1;
        private Stopwatch stopwatch;

        public void start()
        {
            _marketDataEngineClient=new MarketDataEngineClient();
            stopwatch=new Stopwatch();
            _marketDataEngineClient.ServerConnected += _marketDataEngineClient_ServerConnected;
            _marketDataEngineClient.LogonArrived += _marketDataEngineClient_LogonArrived;
            _marketDataEngineClient.InquiryResponseArrived += _marketDataEngineClient_InquiryResponseArrived;
            _marketDataEngineClient.TickArrived += TickArrived;
            _marketDataEngineClient.Start();
           

            

           
        }

        void _marketDataEngineClient_ServerConnected()
        {
            _marketDataEngineClient.SendLoginRequest(new Login() { MarketDataProvider = Common.Core.Constants.MarketDataProvider.Simulated });
        }

        void _marketDataEngineClient_InquiryResponseArrived(Common.Core.ValueObjects.Inquiry.MarketDataProviderInfo obj)
        {
            Subscribe subscribe = new Subscribe();
            subscribe.MarketDataProvider = Common.Core.Constants.MarketDataProvider.Simulated;
            subscribe.Security = new Security() { Symbol = "IBM" };
            _marketDataEngineClient.SendTickSubscriptionRequest(subscribe);
        }

        void _marketDataEngineClient_LogonArrived(string obj)
        {
            _marketDataEngineClient.SendInquiryRequest(Common.Core.Constants.MarketDataProvider.Simulated);
        }

        void TickArrived(Common.Core.DomainModels.Tick obj)
        {  
            if (count == 1)
                {
                    stopwatch.Start();
                    Logger.Info("First tick arrived " + obj, _type.FullName, "TickArrived");
                }
            if (count == 1000000)
            {
                stopwatch.Stop();
                Logger.Info("Last tick arrived " + obj, _type.FullName, "TickArrived");
                Logger.Info("1000000 Ticks recevied in " + stopwatch.ElapsedMilliseconds + " ms", _type.FullName, "TickArrived");
                Logger.Info(1000000 / stopwatch.ElapsedMilliseconds * 1000 + "msg/sec", _type.FullName, "TickArrived");
                _marketDataEngineClient.SendLogoutRequest(new Logout { MarketDataProvider = Common.Core.Constants.MarketDataProvider.Simulated });
                Close();
               
            }

            count++;
            


        }
        public void Close()
        {
            _marketDataEngineClient.Shutdown();
            
        }
    }
}
