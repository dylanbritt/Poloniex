using ConsoleApplication.Helper;
using Poloniex.Log;
using System;

namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Started: {DateTime.Now}");

            try
            {
                RegressionTester.Test();
                //AutoLoader.GetData();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception occurred!");
                Logger.WriteException(exception);
            }

            Console.WriteLine($"Complete: {DateTime.Now}");
            Console.ReadLine();
        }
    }
}
