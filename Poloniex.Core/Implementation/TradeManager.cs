using Poloniex.Api.Implementation;
using Poloniex.Core.Domain.Constants;
using Poloniex.Core.Domain.Constants.Poloniex;
using Poloniex.Core.Domain.Models;
using Poloniex.Core.Utility;
using Poloniex.Data.Contexts;
using Poloniex.Log;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Poloniex.Core.Implementation
{
    public static class TradeManager
    {
        private static readonly object _syncRoot = new object();

        private static int _numberOfHoldings = 0;
        private static int _numberOfTraders
        {
            get
            {
                int result;
                using (var db = new PoloniexContext())
                {
                    result = db.EventActions.Where(x => x.EventActionType == EventActionType.ProcessTradeOrder && x.Task.TaskLoop.LoopStatus == LoopStatus.Started).Count();
                }
                return result;
            }
        }
        private static decimal _percentageOfUsdtBalance = 0.97M;

        public static void BuyCurrencyFromUsdt(string currencyPair, ref TradeSignalOrder tradeSignalOrder)
        {
            lock (_syncRoot)
            {
                decimal percentToTrade = GetPercentToTrade();

                bool isMoving = false;
                int attemptCount = 0;
                Dictionary<string, decimal> balances = PoloniexExchangeService.Instance.ReturnBalances();
                decimal usdtBalance = balances[CurrencyConstants.USDT] * percentToTrade;

                /* don't attempt to trade under $1 */
                if (usdtBalance < 1M)
                {
                    tradeSignalOrder.PlaceValueTradedAt = -1;
                    tradeSignalOrder.MoveValueTradedAt = -1;
                    tradeSignalOrder.LastValueAtProcessing = -1;
                    Logger.Write($"Order: Insufficient funds", Logger.LogType.TransactionLog);
                }

                long orderNumber = 0;

                while (true)
                {
                    attemptCount++;
                    Dictionary<string, Dictionary<string, decimal>> res = PoloniexExchangeService.Instance.ReturnTicker();
                    var latestUsdtBtcTicker = res[currencyPair];

                    // last
                    decimal usdtBtcLastPrice = latestUsdtBtcTicker[TickerResultKeys.last];

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
                        tradeSignalOrder.PlaceValueTradedAt = buyRate;
                        // BUY
                        var buyResult = PoloniexExchangeService.Instance.Buy(currencyPair, buyRate, buyAmount, false, false, true);
                        orderNumber = buyResult.orderNumber;
                        Logger.Write($"Order: Purchasing BTC from USDT; buyRate: {buyRate}, buyAmount: {buyAmount}, _numberOfTraders: {_numberOfTraders}, _numberOfHoldings: {_numberOfHoldings}", Logger.LogType.TransactionLog);
                        isMoving = true;
                    }
                    else
                    {
                        tradeSignalOrder.MoveValueTradedAt = buyRate;
                        // MOVE
                        var moveResult = PoloniexExchangeService.Instance.MoveOrder(orderNumber, buyRate, buyAmount, false, false, true);
                        orderNumber = moveResult.orderNumber;
                        Logger.Write($"Order: Moving (attemptCount:{attemptCount}) BTC from USDT; buyRate: {buyRate}, buyAmount: {buyAmount}, _numberOfTraders: {_numberOfTraders}, _numberOfHoldings: {_numberOfHoldings}", Logger.LogType.TransactionLog);
                    }

                    Thread.Sleep(10 * 1000); // allow exchange to resolve order

                    // Get open orders
                    var openOrders = PoloniexExchangeService.Instance.ReturnOpenOrders(currencyPair);
                    var originalBuyOrder = openOrders.SingleOrDefault(x => x[OpenOrderKeys.orderNumber] == orderNumber.ToString());

                    bool isTradeComplete = originalBuyOrder == null;
                    if (isTradeComplete)
                    {
                        tradeSignalOrder.LastValueAtProcessing = buyRate;
                        _numberOfHoldings++;
                        break;
                    }
                }
            }
        }

        public static void SellCurrencyToUsdt(string currencyPair, ref TradeSignalOrder tradeSignalOrder)
        {
            lock (_syncRoot)
            {
                decimal percentToTrade = 1.00M; // always sell 100%

                bool isMoving = false;
                int attemptCount = 0;
                Dictionary<string, decimal> balances = PoloniexExchangeService.Instance.ReturnBalances();
                decimal currencyBalance = balances[CurrencyUtility.GetCurrencyFromUsdtCurrencyPair(currencyPair)] * percentToTrade;
                long orderNumber = 0;

                while (true)
                {
                    attemptCount++;
                    Dictionary<string, Dictionary<string, decimal>> res = PoloniexExchangeService.Instance.ReturnTicker();
                    var latestUsdtBtcTicker = res[currencyPair];

                    // last
                    decimal usdtBtcLastPrice = latestUsdtBtcTicker[TickerResultKeys.last];

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

                    decimal sellAmount = currencyBalance;

                    if (!isMoving)
                    {
                        tradeSignalOrder.PlaceValueTradedAt = sellRate;
                        // SELL
                        var sellResult = PoloniexExchangeService.Instance.Sell(currencyPair, sellRate, sellAmount, false, false, true);
                        orderNumber = sellResult.orderNumber;
                        Logger.Write($"Order: Selling BTC to USDT; sellRate: {sellRate}, sellAmount: {sellAmount}, _numberOfTraders: {_numberOfTraders}, _numberOfHoldings: {_numberOfHoldings}", Logger.LogType.TransactionLog);
                        isMoving = true;
                    }
                    else
                    {
                        tradeSignalOrder.MoveValueTradedAt = sellRate;
                        // MOVE
                        var moveResult = PoloniexExchangeService.Instance.MoveOrder(orderNumber, sellRate, sellAmount, false, false, true);
                        orderNumber = moveResult.orderNumber;
                        Logger.Write($"Order: Moving (attemptCount:{attemptCount}) BTC to USDT; sellRate: {sellRate}, sellAmount: {sellAmount}, _numberOfTraders: {_numberOfTraders}, _numberOfHoldings: {_numberOfHoldings}", Logger.LogType.TransactionLog);
                    }

                    Thread.Sleep(10 * 1000); // allow exchange to resolve order

                    // Get open orders
                    var openOrders = PoloniexExchangeService.Instance.ReturnOpenOrders(currencyPair);
                    var originalBuyOrder = openOrders.SingleOrDefault(x => x[OpenOrderKeys.orderNumber] == orderNumber.ToString());

                    bool isTradeComplete = originalBuyOrder == null;
                    if (isTradeComplete)
                    {
                        tradeSignalOrder.LastValueAtProcessing = sellRate;
                        _numberOfHoldings--;
                        break;
                    }
                }
            }
        }

        private static decimal GetPercentToTrade(int? numberOfTraders = null)
        {
            numberOfTraders = numberOfTraders ?? _numberOfTraders;

            decimal percentToTrade;
            if (_numberOfHoldings == 0)
            {
                percentToTrade = (_percentageOfUsdtBalance / (decimal)numberOfTraders);
            }
            else
            {
                decimal originalPercentage = _percentageOfUsdtBalance / (decimal)numberOfTraders;
                percentToTrade = (_percentageOfUsdtBalance * originalPercentage) / (_percentageOfUsdtBalance - ((_percentageOfUsdtBalance * originalPercentage) * (decimal)_numberOfHoldings));
            }
            return percentToTrade;
        }

        public static decimal GetPercentToTradeTest(decimal percentOfUsdtBalance, int numberOfTraders, int numberOfHoldings)
        {
            _percentageOfUsdtBalance = percentOfUsdtBalance;
            _numberOfHoldings = numberOfHoldings;
            return GetPercentToTrade(numberOfTraders);
        }
    }
}
