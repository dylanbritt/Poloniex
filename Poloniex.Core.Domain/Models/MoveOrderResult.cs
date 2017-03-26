namespace Poloniex.Core.Domain.Models
{
    public class MoveOrderResult
    {
        public bool success { get; set; }

        public long orderNumber { get; set; }

        public string resultingTrades { get; set; }
    }
}
