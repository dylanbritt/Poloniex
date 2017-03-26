using Newtonsoft.Json;
using Poloniex.Core.Domain.Models;
using Poloniex.Log;
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
        private const string ReturnTickerUrl = "public?command=returnTicker";
        private const string ReturnTradeHistoryTemplateUrl = "public?command=returnTradeHistory&currencyPair={0}&start={1}&end={2}";
        private const string TradingApiUrl = "tradingApi";

        private PoloniexExchangeService() { }

        public static PoloniexExchangeService Instance { get { return _instance; } }

        private class Commands
        {
            public const string ReturnBalances = "returnBalances";
            public const string ReturnOpenOrders = "returnOpenOrders";
            public const string Buy = "buy";
            public const string Sell = "sell";
            public const string MoveOrder = "moveOrder";
        }

        private string PostCommand(string command, Dictionary<string, object> dictionary = null)
        {
            if (dictionary == null)
                dictionary = new Dictionary<string, object>();

            var uri = $"{BaseUrl}/{TradingApiUrl}";

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

        public Dictionary<string, Dictionary<string, decimal>> ReturnTicker()
        {
            lock (_syncRoot)
            {
                var client = new RestClient(BaseUrl);

                short count = 0;
                while (true)
                {
                    var request = new RestRequest(ReturnTickerUrl, Method.GET);

                    Thread.Sleep(175); // throttle api calls to avoid ban
                    IRestResponse<Dictionary<string, Dictionary<string, decimal>>> response = client.Execute<Dictionary<string, Dictionary<string, decimal>>>(request);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        return response.Data;

                    if (count == 2)
                        throw new InvalidOperationException($"RestRequest exceeded three attempts.");

                    count++;
                }
            }
        }

        // public api
        public List<TradeOrder> ReturnTradeHistory(string currencyPair, DateTime startUtcTime, DateTime endUtcTime)
        {
            lock (_syncRoot)
            {
                var client = new RestClient(BaseUrl);

                short count = 0;
                while (true)
                {
                    var request = new RestRequest(string.Format(ReturnTradeHistoryTemplateUrl, currencyPair, startUtcTime.ToUnixDateTime(), endUtcTime.ToUnixDateTime()), Method.GET);

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

        // user api
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
                    catch (Exception exception)
                    {
                        Logger.WriteException(exception);
                        if (count == 2)
                            throw new InvalidOperationException($"PostCommand exceeded three attempts.");

                        count++;
                    }

                    // ################################################################
                }
            }
        }

        public List<Dictionary<string, string>> ReturnOpenOrders(string currencyPair)
        {
            lock (_syncRoot)
            {
                short count = 0;
                while (true)
                {
                    // ################################################################

                    try
                    {
                        Dictionary<string, object> parameters = new Dictionary<string, object>();
                        parameters.Add("currencyPair", currencyPair);

                        Thread.Sleep(175); // throttle api calls to avoid ban
                        var res = PostCommand(Commands.ReturnOpenOrders, parameters);

                        return JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(res);
                    }
                    catch (Exception exception)
                    {
                        Logger.WriteException(exception);
                        if (count == 2)
                            throw new InvalidOperationException($"PostCommand exceeded three attempts.");

                        count++;
                    }

                    // ################################################################
                }
            }
        }

        public BuySellTradeResult Buy(string currencyPair, decimal rate, decimal amount, bool fillOrKill = false, bool immediateOrCancel = false, bool postOnly = false)
        {
            lock (_syncRoot)
            {
                short count = 0;
                while (true)
                {
                    // ################################################################

                    try
                    {
                        Dictionary<string, object> parameters = new Dictionary<string, object>();
                        parameters.Add("currencyPair", currencyPair);
                        parameters.Add("rate", rate);
                        parameters.Add("amount", amount);
                        if (fillOrKill)
                        {
                            parameters.Add("fillOrKill", 1);
                        }
                        if (immediateOrCancel)
                        {
                            parameters.Add("immediateOrCancel", 1);
                        }
                        if (postOnly)
                        {
                            parameters.Add("postOnly", 1);
                        }

                        Thread.Sleep(175); // throttle api calls to avoid ban
                        var res = PostCommand(Commands.Buy, parameters);
                        Logger.Write($"{Commands.Buy}: {res}", Logger.LogType.RestLog);

                        return JsonConvert.DeserializeObject<BuySellTradeResult>(res);
                    }
                    catch (Exception exception)
                    {
                        Logger.WriteException(exception);
                        if (count == 2)
                            throw new InvalidOperationException($"PostCommand exceeded three attempts.");

                        count++;
                    }

                    // ################################################################
                }
            }
        }

        public BuySellTradeResult Sell(string currencyPair, decimal rate, decimal amount, bool fillOrKill = false, bool immediateOrCancel = false, bool postOnly = false)
        {
            lock (_syncRoot)
            {
                short count = 0;
                while (true)
                {
                    // ################################################################

                    try
                    {
                        Dictionary<string, object> parameters = new Dictionary<string, object>();
                        parameters.Add("currencyPair", currencyPair);
                        parameters.Add("rate", rate);
                        parameters.Add("amount", amount);
                        if (fillOrKill)
                        {
                            parameters.Add("fillOrKill", 1);
                        }
                        if (immediateOrCancel)
                        {
                            parameters.Add("immediateOrCancel", 1);
                        }
                        if (postOnly)
                        {
                            parameters.Add("postOnly", 1);
                        }

                        Thread.Sleep(175); // throttle api calls to avoid ban
                        var res = PostCommand(Commands.Sell, parameters);
                        Logger.Write($"{Commands.Sell}: {res}", Logger.LogType.RestLog);

                        return JsonConvert.DeserializeObject<BuySellTradeResult>(res);
                    }
                    catch (Exception exception)
                    {
                        Logger.WriteException(exception);
                        if (count == 2)
                            throw new InvalidOperationException($"PostCommand exceeded three attempts.");

                        count++;
                    }

                    // ################################################################
                }
            }
        }

        public MoveOrderTradeResult MoveOrder(long orderNumber, decimal rate, decimal amount)
        {
            lock (_syncRoot)
            {
                short count = 0;
                while (true)
                {
                    // ################################################################

                    try
                    {
                        Dictionary<string, object> parameters = new Dictionary<string, object>();
                        parameters.Add("orderNumber", orderNumber);
                        parameters.Add("rate", rate);
                        parameters.Add("amount", amount);

                        Thread.Sleep(175); // throttle api calls to avoid ban
                        var res = PostCommand(Commands.MoveOrder, parameters);
                        Logger.Write($"{Commands.MoveOrder}: {res}", Logger.LogType.RestLog);

                        return JsonConvert.DeserializeObject<MoveOrderTradeResult>(res);
                    }
                    catch (Exception exception)
                    {
                        Logger.WriteException(exception);
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