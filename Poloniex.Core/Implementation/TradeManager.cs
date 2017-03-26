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
                Dictionary<string, decimal> balances = PoloniexExchangeService.Instance.ReturnBalances();
                decimal usdtBalance = balances[CurrencyConstants.USDT] * percent;
                long orderNumber = 0;

                while (true)
                {
                    Dictionary<string, Dictionary<string, decimal>> res = PoloniexExchangeService.Instance.ReturnTicker();
                    var latestUsdtBtcTicker = res[CurrencyPairConstants.USDT_BTC];

                    // seller
                    decimal usdtBtcLowestAsk = latestUsdtBtcTicker[TickerResultKeys.LowestAsk];

                    decimal buyRate = (0.75M) * usdtBtcLowestAsk; // TODO: FIX!!!
                    if (isMoving)
                    {
                        buyRate = buyRate * 0.75M;
                    }

                    decimal buyAmount = usdtBalance / buyRate;

                    if (!isMoving)
                    {
                        // BUY
                        var buyResult = PoloniexExchangeService.Instance.Buy(CurrencyPairConstants.USDT_BTC, buyRate, buyAmount, false, false, true);
                        orderNumber = buyResult.orderNumber;
                        Logger.Write($"Order: Purchasing BTC from USDT; buyRate: {buyRate}, buyAmount: {buyAmount}");
                        isMoving = true;
                    }
                    else
                    {
                        // MOVE
                        var moveResult = PoloniexExchangeService.Instance.MoveOrder(orderNumber, buyRate, buyAmount);
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

        public static void SellBtcToUsdt()
        {

        }
    }
}
