using System;
using System.Collections.Generic;

namespace Poloniex.Core.Domain.Models.Poloniex
{
    public class ReturnTradeHistoryResult
    {
        public List<TradeOrder> TradeOrders { get; set; }

        public class TradeOrder
        {
            public DateTime date { get; set; }
            public string type { get; set; }
            public decimal rate { get; set; }
        }
    }
}
