using System.Collections.Generic;
using System.Linq;

namespace Poloniex.Core.Implementation
{
    public static class MovingAverageCalculations
    {
        public static void Bind()
        {

        }

        public static decimal CalculateSma(List<decimal> closingValues)
        {
            return closingValues.Sum() / closingValues.Count;
        }

        public static decimal CalculateEma(decimal closingValue, decimal previousEma, int intervalCount)
        {
            var multipler = 2M / ((decimal)intervalCount + 1M);

            return ((closingValue - previousEma) * multipler) + previousEma;
        }
    }
}
