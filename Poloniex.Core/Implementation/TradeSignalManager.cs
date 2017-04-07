using Poloniex.Core.Domain.Constants;
using Poloniex.Core.Domain.Models;
using Poloniex.Data.Contexts;
using Poloniex.Log;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Poloniex.Core.Implementation
{
    public static class TradeSignalManager
    {
        private static Dictionary<string, TradeSignalConfiguration> Configurations = new Dictionary<string, TradeSignalConfiguration>();

        private static Dictionary<string, TradeSignalRegistry> Signals = new Dictionary<string, TradeSignalRegistry>();

        public static void InitProcessEmaCrossOverSignal(string currencyPair, TradeSignalConfiguration tradeSignalConfiguration)
        {
            Configurations[currencyPair] = tradeSignalConfiguration;
            Signals[currencyPair] = new TradeSignalRegistry();
        }

        public static void ProcessEmaCrossOverSignal(Guid eventActionId)
        {
            using (var db = new PoloniexContext())
            {
                var tradeSignalEventAction = db.TradeSignalEventActions
                    .Single(x => x.EventActionId == eventActionId);

                var currencyPair = tradeSignalEventAction.CurrencyPair;

                var latestShorterMovingAverage = db.MovingAverages
                    .Where(x =>
                        x.CurrencyPair == tradeSignalEventAction.CurrencyPair &&
                        x.Interval == tradeSignalEventAction.ShorterMovingAverageInterval)
                    .OrderByDescending(x => x.ClosingDateTime)
                    .First();

                var latestLongerMovingAverage = db.MovingAverages
                    .Where(x =>
                        x.CurrencyPair == tradeSignalEventAction.CurrencyPair &&
                        x.Interval == tradeSignalEventAction.LongerMovingAverageInterval)
                    .OrderByDescending(x => x.ClosingDateTime)
                    .First();

                var lastClosingValue = db.CurrencyDataPoints
                    .Where(x => x.CurrencyPair == tradeSignalEventAction.CurrencyPair)
                    .OrderByDescending(x => x.ClosingDateTime)
                    .First().ClosingValue;

                Signals[currencyPair].IsBullish = latestShorterMovingAverage.MovingAverageValue - latestLongerMovingAverage.MovingAverageValue >= 0;

                if (!Signals[currencyPair].Init)
                {

                    if (!Signals[currencyPair].WasBullish && Signals[currencyPair].IsBullish && !Signals[currencyPair].HasHoldings)
                    {
                        // BUY
                        Signals[currencyPair].ShouldBuy = true;
                        Signals[currencyPair].BuyValue = lastClosingValue;
                    }

                    if (Signals[currencyPair].HasHoldings)
                    {
                        decimal high;
                        decimal low;

                        high = Signals[currencyPair].BuyValue * (1M + Configurations[currencyPair].StopLossPercentageUpper);
                        low = Signals[currencyPair].BuyValue * (1M - Configurations[currencyPair].StopLossPercentageLower);

                        if (Configurations[currencyPair].IsStopLossTailing)
                        {
                            if (lastClosingValue >= high)
                            {
                                Signals[currencyPair].BuyValue = high * (1M + Configurations[currencyPair].StopLossPercentageUpper);
                            }
                            if (lastClosingValue <= low)
                            {
                                Signals[currencyPair].ShouldSell = true;
                            }
                        }
                        else
                        {
                            if (lastClosingValue >= high || lastClosingValue <= low)
                            {
                                Signals[currencyPair].ShouldSell = true;
                            }
                        }
                    }

                    if (Signals[currencyPair].ShouldBuy)
                    {
                        var buyTradeOrder = new TradeOrderEventAction()
                        {
                            TradeOrderType = TradeOrderType.Buy,
                            LastValueAtRequest = latestShorterMovingAverage.LastClosingValue,
                            IsProcessed = false,
                            InProgress = false,
                            OrderRequestedDateTime = DateTime.UtcNow
                        };
                        db.TradeOrderEventActions.Add(buyTradeOrder);
                        db.SaveChanges();
                        Signals[currencyPair].HasHoldings = true;
                    }

                    if (Signals[currencyPair].ShouldSell)
                    {
                        var sellTradeOrder = new TradeOrderEventAction()
                        {
                            TradeOrderType = TradeOrderType.Sell,
                            LastValueAtRequest = latestShorterMovingAverage.LastClosingValue,
                            IsProcessed = false,
                            InProgress = false,
                            OrderRequestedDateTime = DateTime.UtcNow
                        };
                        db.TradeOrderEventActions.Add(sellTradeOrder);
                        db.SaveChanges();
                        Signals[currencyPair].HasHoldings = false;
                    }

                    Logger.Write($"wasBullish: {Signals[currencyPair].WasBullish}, isBullish: {Signals[currencyPair].IsBullish}, hasHolding: {Signals[currencyPair].HasHoldings}, shouldBuy {Signals[currencyPair].ShouldBuy}, shouldSell {Signals[currencyPair].ShouldSell}", Logger.LogType.TransactionLog);
                    Signals[currencyPair].ShouldBuy = false;
                    Signals[currencyPair].ShouldSell = false;
                }
                else
                {
                    Logger.Write($"TradeTask init, evenActionId: {eventActionId} (see TransactionLog)", Logger.LogType.ServiceLog);
                    Signals[currencyPair].Init = false;
                }

                Signals[currencyPair].WasBullish = Signals[currencyPair].IsBullish;
            }
        }
    }
}
