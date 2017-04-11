using System;
using System.Text;

namespace ConsoleApplication.Helper
{
    public class RegressionResults
    {
        public string CurrencyPair { get; set; }

        public int MinutesPerInterval { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public decimal DataMin { get; set; }
        public decimal DataMax { get; set; }
        public decimal DataMean { get; set; }
        public decimal DataMedian { get; set; }
        public decimal DataStd { get; set; }

        public decimal ProfitMin { get; set; }
        public decimal ProfitMax { get; set; }
        public decimal ProfitMean { get; set; }
        public decimal ProfitMedian { get; set; }
        public decimal ProfitStd { get; set; }

        public decimal PercentageChange { get; set; }
        public decimal TotalProfit { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"CurrencyPair: {CurrencyPair} -> \n");
            sb.Append($"\tMinutesPerInterval: {MinutesPerInterval}, \n");
            sb.Append($"\tStartDateTime: {StartDateTime}, EndDateTime: {EndDateTime}, \n");
            sb.Append($"\tDataMin: {DataMin}, DataMax: {DataMax}, DataMean: {DataMean}, DataMedian: {DataMedian}, DataStd: {DataStd}, \n");
            sb.Append($"\tProfitMin: {ProfitMin}, ProfitMax: {ProfitMax}, ProfitMean: {ProfitMean}, ProfitMedian: {ProfitMedian}, ProfitStd: {ProfitStd}, \n");
            sb.Append($" --> PercentageChange: {Math.Round(PercentageChange, 4)} TotalProfit: {TotalProfit} \n");

            return sb.ToString();
        }
    }
}
