using System;

namespace Poloniex.Core.Domain.Models.Poloniex
{
    public class BuySellResultingTrade
    {
        decimal amount { get; set; }

        DateTime date { get; set; }

        decimal rate { get; set; }

        decimal total { get; set; }

        int tradeID { get; set; }

        string type { get; set; }
    }
}
