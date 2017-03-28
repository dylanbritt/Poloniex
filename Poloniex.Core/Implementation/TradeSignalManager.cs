using Poloniex.Core.Domain.Constants;
using Poloniex.Core.Domain.Models;
using Poloniex.Data.Contexts;
using Poloniex.Log;
using System;
using System.Data.Entity;
using System.Linq;

namespace Poloniex.Core.Implementation
{
    public static class TradeSignalManager
    {
        private static bool _init = true;

        private static bool _wasBullish = false;
        private static bool _isBullish = false;

        public static bool _hasHoldings = false;

        private static bool _shouldBuy = false;
        private static bool _shouldSell = false;

        public static void ProcessTradeSignalEventAction(Guid eventActionId)
        {
            using (var db = new PoloniexContext())
            {
                var tradeSignalEventAction = db.TradeSignalEventActions
                    .Include(x => x.SignalMovingAverage)
                    .Include(x => x.BaseMovingAverage)
                    .Single(x => x.EventActionId == eventActionId);

                var latestSignalMovingAverage = db.MovingAverages
                    .Where(x =>
                        x.CurrencyPair == tradeSignalEventAction.SignalMovingAverage.CurrencyPair &&
                        x.Interval == tradeSignalEventAction.SignalMovingAverage.Interval)
                    .OrderByDescending(x => x.ClosingDateTime)
                    .First();

                var latestBaseMovingAverage = db.MovingAverages
                    .Where(x =>
                        x.CurrencyPair == tradeSignalEventAction.BaseMovingAverage.CurrencyPair &&
                        x.Interval == tradeSignalEventAction.BaseMovingAverage.Interval)
                    .OrderByDescending(x => x.ClosingDateTime)
                    .First();

                _isBullish = latestSignalMovingAverage.MovingAverageClosingValue > latestBaseMovingAverage.MovingAverageClosingValue;

                if (!_init)
                {

                    if (!_wasBullish && _isBullish && !_hasHoldings)
                    {
                        // BUY
                        _shouldBuy = true;
                    }
                    if (_wasBullish && !_isBullish && _hasHoldings)
                    {
                        // SELL
                        _shouldSell = true;
                    }

                    if (_shouldBuy)
                    {
                        var buyTradeSignalOrder = new TradeSignalOrder()
                        {
                            TradeSignalOrderType = TradeSignalOrderType.Buy,
                            LastValueAtRequest = latestSignalMovingAverage.LastClosingValue,
                            IsProcessed = false,
                            InProgress = false,
                            OrderRequestedDateTime = DateTime.UtcNow
                        };
                        db.TradeSignalOrders.Add(buyTradeSignalOrder);
                        db.SaveChanges();
                        _hasHoldings = true;
                    }

                    if (_shouldSell)
                    {
                        var sellTradeSignalOrder = new TradeSignalOrder()
                        {
                            TradeSignalOrderType = TradeSignalOrderType.Sell,
                            LastValueAtRequest = latestSignalMovingAverage.LastClosingValue,
                            IsProcessed = false,
                            InProgress = false,
                            OrderRequestedDateTime = DateTime.UtcNow
                        };
                        db.TradeSignalOrders.Add(sellTradeSignalOrder);
                        db.SaveChanges();
                        _hasHoldings = false;
                    }

                    Logger.Write($"wasBullish: {_wasBullish}, isBullish: {_isBullish}, hasHolding: {_hasHoldings}, shouldBuy {_shouldBuy}, shouldSell {_shouldSell}", Logger.LogType.ServiceLog);
                }

                _wasBullish = _isBullish;
            }
        }
    }
}
