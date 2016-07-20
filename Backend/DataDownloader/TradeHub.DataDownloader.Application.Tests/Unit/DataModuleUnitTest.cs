using System;
using NUnit.Framework;
using TradeHub.Common.Core.Constants;
using TradeHub.Common.Core.DomainModels;
using TradeHub.DataDownloader.Common.ConcreteImplementation;
using TradeHub.DataDownloader.UserInterface.Common;
using TradeHub.DataDownloader.UserInterface.Common.Messages;
using TradeHub.DataDownloader.UserInterface.DataModule.ViewModel;

namespace TradeHub.DataDownloader.Application.Tests.Unit
{
    [TestFixture]
    public class DataModuleUnitTest
    {
        /// <summary>
        /// Test LogonArrived
        /// </summary>
        [Test]
        public void LoginArrivedTest()
        {
            DataViewModel viewModel=new DataViewModel();
            foreach (var selectedProvider in viewModel.SelectedProviders)
            {
                if (selectedProvider.ProviderName==MarketDataProvider.Blackwood)
                {
                    Assert.IsFalse(selectedProvider.IsConnected);
                }
                
            }
            EventSystem.Publish<LoginArrivedMessage>(new LoginArrivedMessage{Provider = new Provider{IsConnected = true,ProviderName = MarketDataProvider.Blackwood}});
            foreach (var selectedProvider in viewModel.SelectedProviders)
            {
                if (selectedProvider.ProviderName == MarketDataProvider.Blackwood)
                {
                    Assert.IsTrue(selectedProvider.IsConnected);
                }
            }
        }

        /// <summary>
        /// Test New Symbol Subscribe
        /// </summary>
        [Test]
        public void SubscribeToNewSymbol()
        {
            DataViewModel viewModel = new DataViewModel();
            EventSystem.Publish<LoginArrivedMessage>(new LoginArrivedMessage { Provider = new Provider { IsConnected = true, ProviderName = MarketDataProvider.Blackwood } });
            SecurityPermissions securityPermissions = new SecurityPermissions { Id = Guid.NewGuid().ToString(), MarketDataProvider = MarketDataProvider.Blackwood ,Security = new Security{Symbol = "AAPL"}};
            viewModel.SelectedProvider = new Provider { IsConnected = true, ProviderName = MarketDataProvider.Blackwood };
            EventSystem.Publish<SecurityPermissions>(securityPermissions);
            
            Assert.AreEqual(1,viewModel.SecurityStatDictionary[MarketDataProvider.Blackwood].Count);
        }
    }
}
