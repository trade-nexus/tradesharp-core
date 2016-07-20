using System;
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
