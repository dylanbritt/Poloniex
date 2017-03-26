using Poloniex.Api.Implementation;
using Poloniex.Core.Domain.Constants;
using Poloniex.Log;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Poloniex.Core.Implementation
{
    public class TradeManager
    {
        private static readonly object _syncRoot = new object();

        public static void BuyBtcFromUsdt(decimal percent = 1.00M)
        {
            lock (_syncRoot)
            {
                bool isMoving = false;
                int attemptCount = 0;
                Dictionary<string, decimal> balances = PoloniexExchangeService.Instance.ReturnBalances();
                decimal usdtBalance = balances[CurrencyConstants.USDT] * percent;
                long orderNumber = 0;

                while (true)
                {
                    attemptCount++;
                    Dictionary<string, Dictionary<string, decimal>> res = PoloniexExchangeService.Instance.ReturnTicker();
                    var latestUsdtBtcTicker = res[CurrencyPairConstants.USDT_BTC];

                    // last
                    decimal usdtBtcLastPrice = latestUsdtBtcTicker[TickerResultKeys.Last];

                    /* TESTING CODE : BEGIN */
                    //decimal buyRate = (0.75M) * usdtBtcLastPrice; // TODO: FIX!!!
                    //if (isMoving)
                    //{
                    //    buyRate = buyRate * 0.75M;
                    //}
                    /* TESTING CODE : END */

                    /* PRODUCTION CODE : BEGIN */
                    decimal buyRate = usdtBtcLastPrice;
                    /* PRODUCTION CODE : END */

                    decimal buyAmount = usdtBalance / buyRate;

                    if (!isMoving)
                    {
                        // BUY
                        var buyResult = PoloniexExchangeService.Instance.Buy(CurrencyPairConstants.USDT_BTC, buyRate, buyAmount);
                        orderNumber = buyResult.orderNumber;
                        Logger.Write($"Order: Purchasing BTC from USDT; buyRate: {buyRate}, buyAmount: {buyAmount}", Logger.LogType.TransactionLog);
                        isMoving = true;
                    }
                    else
                    {
                        // MOVE
                        var moveResult = PoloniexExchangeService.Instance.MoveOrder(orderNumber, buyRate, buyAmount);
                        orderNumber = moveResult.orderNumber;
                        Logger.Write($"Order: Moving (attemptCount:{attemptCount}) BTC from USDT; buyRate: {buyRate}, buyAmount: {buyAmount}", Logger.LogType.TransactionLog);
                    }

                    Thread.Sleep(10 * 1000); // allow exchange to resolve order

                    // Get open orders
                    var openOrders = PoloniexExchangeService.Instance.ReturnOpenOrders(CurrencyPairConstants.USDT_BTC);
                    var originalBuyOrder = openOrders.SingleOrDefault(x => x[OpenOrderKeys.OrderNumber] == orderNumber.ToString());

                    bool isTradeComplete = originalBuyOrder == null;
                    if (isTradeComplete)
                    {
                        break;
                    }
                }
            }
        }

        public static void SellBtcToUsdt(decimal percent = 1.00M)
        {
            lock (_syncRoot)
            {
                bool isMoving = false;
                int attemptCount = 0;
                Dictionary<string, decimal> balances = PoloniexExchangeService.Instance.ReturnBalances();
                decimal btcBalance = balances[CurrencyConstants.BTC] * percent;
                long orderNumber = 0;

                while (true)
                {
                    attemptCount++;
                    Dictionary<string, Dictionary<string, decimal>> res = PoloniexExchangeService.Instance.ReturnTicker();
                    var latestUsdtBtcTicker = res[CurrencyPairConstants.USDT_BTC];

                    // last
                    decimal usdtBtcLastPrice = latestUsdtBtcTicker[TickerResultKeys.Last];

                    /* TESTING CODE : BEGIN */
                    //decimal sellRate = (1.25M) * usdtBtcLastPrice; // TODO: FIX!!!
                    //if (isMoving)
                    //{
                    //    sellRate = sellRate * 1.25M;
                    //}
                    /* TESTING CODE : END */

                    /* PRODUCTION CODE : BEGIN */
                    decimal sellRate = usdtBtcLastPrice;
                    /* PRODUCTION CODE : END */

                    decimal sellAmount = btcBalance;

                    if (!isMoving)
                    {
                        // SELL
                        var sellResult = PoloniexExchangeService.Instance.Sell(CurrencyPairConstants.USDT_BTC, sellRate, sellAmount);
                        orderNumber = sellResult.orderNumber;
                        Logger.Write($"Order: Selling BTC to USDT; sellRate: {sellRate}, sellAmount: {sellAmount}", Logger.LogType.TransactionLog);
                        isMoving = true;
                    }
                    else
                    {
                        // MOVE
                        var moveResult = PoloniexExchangeService.Instance.MoveOrder(orderNumber, sellRate, sellAmount);
                        orderNumber = moveResult.orderNumber;
                        Logger.Write($"Order: Moving (attemptCount:{attemptCount}) BTC to USDT; sellRate: {sellRate}, sellAmount: {sellAmount}", Logger.LogType.TransactionLog);
                    }

                    Thread.Sleep(10 * 1000); // allow exchange to resolve order

                    // Get open orders
                    var openOrders = PoloniexExchangeService.Instance.ReturnOpenOrders(CurrencyPairConstants.USDT_BTC);
                    var originalBuyOrder = openOrders.SingleOrDefault(x => x[OpenOrderKeys.OrderNumber] == orderNumber.ToString());

                    bool isTradeComplete = originalBuyOrder == null;
                    if (isTradeComplete)
                    {
                        break;
                    }
                }
            }
        }
    }
}
