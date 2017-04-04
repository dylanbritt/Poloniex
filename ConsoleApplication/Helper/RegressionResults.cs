using System;

namespace ConsoleApplication.Helper
{
    public class RegressionResults
    {
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
        public decimal TotalProfit { get; set; }

        public override string ToString()
        {
            return $"StartDateTime: {StartDateTime}, EndDateTime: {EndDateTime},\nDataMin: {DataMin}, DataMax: {DataMax}, DataMean: {DataMean}, DataMedian: {DataMedian}, DataStd: {DataStd},\nProfitMin: {ProfitMin}, ProfitMax: {ProfitMax}, ProfitMean: {ProfitMean}, ProfitMedian: {ProfitMedian}, ProfitStd: {ProfitStd},\nTotalProfit: {TotalProfit}";
        }
    }
}
