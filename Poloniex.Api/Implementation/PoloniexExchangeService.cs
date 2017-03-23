using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading;
using static Poloniex.Core.Domain.ReturnTradeHistoryResult;

namespace Poloniex.Api.Implementation
{
    public static class PoloniexExchangeServiceExtensions
    {
        public static long ToUnixDateTime(this DateTime dt)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, dt.Kind);
            var unixTimestamp = System.Convert.ToInt64((dt - date).TotalSeconds);

            return unixTimestamp;
        }
    }

    public class PoloniexExchangeService
    {
        private readonly object _syncRoot = new object();
        private static readonly PoloniexExchangeService _instance = new PoloniexExchangeService();

        private const string BaseUrl = "https://poloniex.com";
        private const string ReturnTradeHistoryTemplate = "public?command=returnTradeHistory&currencyPair={0}&start={1}&end={2}";

        private PoloniexExchangeService() { }

        public static PoloniexExchangeService Instance { get { return _instance; } }

        public DateTime ToDateTime(long unixTimestamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return dateTime.AddSeconds(unixTimestamp);
        }

        public List<TradeOrder> ReturnTradeHistory(string currencyPair, DateTime startUtcTime, DateTime endUtcTime)
        {
            lock (_syncRoot)
            {
                var client = new RestClient(BaseUrl);

                short count = 0;
                while (true)
                {
                    var request = new RestRequest(string.Format(ReturnTradeHistoryTemplate, currencyPair, startUtcTime.ToUnixDateTime(), endUtcTime.ToUnixDateTime()), Method.GET);

                    Thread.Sleep(5000); // throttle api calls to avoid ban
                    IRestResponse<List<TradeOrder>> response = client.Execute<List<TradeOrder>>(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        return response.Data;

                    if (count == 3)
                        throw new InvalidOperationException($"RestRequest exceeded three attempts.");

                    count++;
                }
            }
        }
    }
}
