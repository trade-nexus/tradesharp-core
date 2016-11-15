/***************************************************************************** 
* Copyright 2016 Aurora Solutions 
* 
*    http://www.aurorasolutions.io 
* 
* Aurora Solutions is an innovative services and product company at 
* the forefront of the software industry, with processes and practices 
* involving Domain Driven Design(DDD), Agile methodologies to build 
* scalable, secure, reliable and high performance products.
* 
* TradeSharp is a C# based data feed and broker neutral Algorithmic 
* Trading Platform that lets trading firms or individuals automate 
* any rules based trading strategies in stocks, forex and ETFs. 
* TradeSharp allows users to connect to providers like Tradier Brokerage, 
* IQFeed, FXCM, Blackwood, Forexware, Integral, HotSpot, Currenex, 
* Interactive Brokers and more. 
* Key features: Place and Manage Orders, Risk Management, 
* Generate Customized Reports etc 
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License. 
* You may obtain a copy of the License at 
* 
*    http://www.apache.org/licenses/LICENSE-2.0 
* 
* Unless required by applicable law or agreed to in writing, software 
* distributed under the License is distributed on an "AS IS" BASIS, 
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
* See the License for the specific language governing permissions and 
* limitations under the License. 
*****************************************************************************/


﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using TradeHub.OrderExecutionProvider.Tradier.ValueObject;

namespace TradeHub.OrderExecutionProvider.Tradier.Utility
{
    /// <summary>
    /// Tradier REST Handler
    /// </summary>
    public class TradierManager
    {
        private RestClient _restClient;
        private string _accountId;
        private string _token;

        public TradierManager(string accountId, string token,string url)
        {
            _restClient = new RestClient(url);
            _accountId = accountId;
            _token = token;
        }

        /// <summary>
        /// Send limit order
        /// </summary>
        /// <param name="side"></param>
        /// <param name="quantity"></param>
        /// <param name="symbol"></param>
        /// <param name="price"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public string SendLimitOrder(string side, int quantity, string symbol, decimal price, string duration)
        {
            var request = CreateRequest("/v1/accounts/" + _accountId + "/orders", Method.POST);
            request.AddParameter("class", "equity");
            request.AddParameter("symbol", symbol);
            request.AddParameter("duration", duration.ToLower());
            request.AddParameter("side", side.ToLower());
            request.AddParameter("quantity", quantity);
            request.AddParameter("type", "limit");
            request.AddParameter("price", price);
            IRestResponse response = _restClient.Execute(request);
            var order = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return order.order.id.ToString();
        }

        /// <summary>
        /// Send Market Order
        /// </summary>
        /// <param name="side"></param>
        /// <param name="quantity"></param>
        /// <param name="symbol"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public string SendMarketOrder(string side, int quantity, string symbol, string duration)
        {
            var request = CreateRequest("/v1/accounts/" + _accountId + "/orders", Method.POST);
            request.AddParameter("class", "equity");
            request.AddParameter("symbol", symbol);
            request.AddParameter("duration", duration.ToLower());
            request.AddParameter("side", side.ToLower());
            request.AddParameter("quantity", quantity);
            request.AddParameter("type", "market");
            IRestResponse response = _restClient.Execute(request);
            var order = JsonConvert.DeserializeObject<dynamic>(response.Content);
            return order.order.id.ToString();
        }

        /// <summary>
        /// Get Order Status
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public TradierOrder GetOrderStatus(string orderId)
        {
            var request = CreateRequest("/v1/accounts/" + _accountId + "/orders/" + orderId, Method.GET);
            IRestResponse<TradierOrder> response = _restClient.Execute<TradierOrder>(request);
            return response.Data;
        }

        /// <summary>
        /// Cancel Order
        /// </summary>
        /// <param name="orderId"></param>
        public HttpStatusCode CancelOrder(string orderId)
        {
            var request = CreateRequest("/v1/accounts/" + _accountId + "/orders/" + orderId, Method.DELETE);
            IRestResponse response = _restClient.Execute(request);
            return response.StatusCode;
        }

        /// <summary>
        /// Get account balance
        /// </summary>
        public HttpStatusCode GetAccountBalance()
        {
            var request = CreateRequest("/v1/accounts/" + _accountId + "/balances", Method.GET);
            IRestResponse response = _restClient.Execute(request);
            return response.StatusCode;
        }

        /// <summary>
        /// Create Request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private RestRequest CreateRequest(string url, Method method)
        {
            var request = new RestRequest(url, method);
            request.AddHeader("Authorization", "Bearer " + _token);
            request.AddHeader("Accept", "application/json");
            return request;
        }
    }
}
