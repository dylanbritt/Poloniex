using Poloniex.Core.Domain.Constants;
using Poloniex.Data.Contexts;
using Poloniex.Log;
using System;
using System.Data.Entity;
using System.Linq;

namespace Poloniex.Core.Implementation
{
    public class TradeOrderManager
    {
        public static void ProcessTradeSignalOrders(Guid eventActionId)
        {
            using (var db = new PoloniexContext())
            {
                var currencyPair = db.EventActions.Single(x => x.EventActionId == eventActionId).TradeOrderEventAction.CurrencyPair;

                // get oldest uncompleted order
                var oldest = db.TradeOrderEventActions
                    .Where(x =>
                        x.CurrencyPair == currencyPair &&
                        !x.IsProcessed)
                    .OrderBy(x => x.OrderRequestedDateTime)
                    .FirstOrDefault();

                if (oldest != null)
                {
                    Logger.Write($"Processing TradeOrderEventActionId: {oldest.TradeOrderEventActionId} for eventActionId: {eventActionId}", Logger.LogType.TransactionLog);

                    oldest.IsProcessed = true;
                    oldest.InProgress = true;
                    db.Entry(oldest).State = EntityState.Modified;
                    db.SaveChanges();

                    if (oldest.TradeOrderType == TradeOrderType.Buy)
                    {
                        TradeManager.BuyCurrencyFromUsdt(currencyPair, ref oldest);
                    }
                    else
                    {
                        TradeManager.SellCurrencyToUsdt(currencyPair, ref oldest);
                    }

                    oldest.OrderCompletedDateTime = DateTime.UtcNow;
                    oldest.InProgress = false;
                    db.Entry(oldest).State = EntityState.Modified;
                    db.SaveChanges();
                }
            };
        }
    }
}
