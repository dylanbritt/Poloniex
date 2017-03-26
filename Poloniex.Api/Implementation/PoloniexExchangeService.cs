using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using static Poloniex.Core.Domain.Models.ReturnTradeHistoryResult;

namespace Poloniex.Api.Implementation
{
    internal static class PoloniexExchangeServiceExtensions
    {
        public static long ToUnixDateTime(this DateTime dt)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, dt.Kind);
            var unixTimestamp = System.Convert.ToInt64((dt - date).TotalSeconds);

            return unixTimestamp;
        }

        public static string ToUrlEncoded(this Dictionary<string, object> dictionary)
        {
            return string.Join("&", dictionary.Select((x) => x.Key + "=" + x.Value.ToString()));
        }

    }

    public class PoloniexExchangeService
    {
        private readonly object _syncRoot = new object();
        private static readonly PoloniexExchangeService _instance = new PoloniexExchangeService();

        private const string BaseUrl = "https://poloniex.com";
        private const string ReturnTradeHistoryTemplate = "public?command=returnTradeHistory&currencyPair={0}&start={1}&end={2}";
        private const string TradingApi = "tradingApi";

        private PoloniexExchangeService() { }

        public static PoloniexExchangeService Instance { get { return _instance; } }

        private class Commands
        {
            public const string ReturnBalances = "returnBalances";
        }

        private string PostCommand(string command, Dictionary<string, object> dictionary = null)
        {
            if (dictionary == null)
                dictionary = new Dictionary<string, object>();

            var uri = $"{BaseUrl}/{TradingApi}";

            /*
             * Headers:
             *  Key
             *  Sign
             *  
             * Parameters:
             *  command
             *  nonce                     
             */

            var apiKey = ConfigurationManager.AppSettings["poloniexApiKey"];
            var secret = ConfigurationManager.AppSettings["poloniexSecret"];
            var nonce = (int)DateTime.UtcNow.ToUnixDateTime();
            dictionary.Add("nonce", nonce);
            dictionary.Add("command", command);

            string postData = dictionary.ToUrlEncoded();

            byte[] secretkey = Encoding.UTF8.GetBytes(secret); // new Byte[128];
            string stringHash = string.Empty;

            using (HMACSHA512 hmac = new HMACSHA512(secretkey))
            {
                var inputToHash = Encoding.UTF8.GetBytes(postData);
                var byteHash = hmac.ComputeHash(inputToHash);
                stringHash = BitConverter.ToString(byteHash).Replace("-", "").ToLower();
            }

            var webClient = new WebClient();
            webClient.Headers.Add("Key", apiKey);
            webClient.Headers.Add("Sign", stringHash);

            webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

            var result = webClient.UploadString(uri, "POST", postData);

            return result;
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

                    Thread.Sleep(175); // throttle api calls to avoid ban
                    IRestResponse<List<TradeOrder>> response = client.Execute<List<TradeOrder>>(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        return response.Data;

                    if (count == 2)
                        throw new InvalidOperationException($"RestRequest exceeded three attempts.");

                    count++;
                }
            }
        }

        public Dictionary<string, decimal> ReturnBalances()
        {
            lock (_syncRoot)
            {
                short count = 0;
                while (true)
                {
                    // ################################################################

                    try
                    {
                        Thread.Sleep(175); // throttle api calls to avoid ban
                        var res = PostCommand(Commands.ReturnBalances);

                        return JsonConvert.DeserializeObject<Dictionary<string, decimal>>(res);
                    }
                    catch
                    {
                        if (count == 2)
                            throw new InvalidOperationException($"PostCommand exceeded three attempts.");

                        count++;
                    }

                    // ################################################################
                }
            }
        }
    }
}