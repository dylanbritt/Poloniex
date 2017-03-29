using Poloniex.Core.Domain.Constants;
using Poloniex.Data.Contexts;
using Poloniex.Log;
using System;
using System.Data.Entity;
using System.Linq;

namespace Poloniex.Core.Implementation
{
    public class TradeSignalOrderManager
    {
        public static void ProcessTradeSignalOrders(Guid eventActionId)
        {
            using (var db = new PoloniexContext())
            {
                // get oldest uncompleted order
                var oldest = db.TradeSignalOrders
                    .Where(x => !x.IsProcessed)
                    .OrderBy(x => x.OrderRequestedDateTime)
                    .FirstOrDefault();

                if (oldest != null)
                {
                    Logger.Write($"Processing TradeSignalOrderId: {oldest.TradeSignalOrderId} for eventActionId: {eventActionId}", Logger.LogType.TransactionLog);

                    oldest.IsProcessed = true;
                    oldest.InProgress = true;
                    db.Entry(oldest).State = EntityState.Modified;
                    db.SaveChanges();

                    if (oldest.TradeSignalOrderType == TradeSignalOrderType.Buy)
                    {
                        TradeManager.BuyBtcFromUsdt(ref oldest);
                    }
                    else
                    {
                        TradeManager.SellBtcToUsdt(ref oldest);
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
