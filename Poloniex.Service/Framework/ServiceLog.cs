using System;
using System.IO;

namespace Poloniex.Service.Framework
{
    public static class ServiceLog
    {
        private const string DATE_TIME_FORMAT = "MM/dd/yyyy hh:mm:ss:fff";

        public static void Write(string message)
        {
            if (ConfigurationHelper.EnableLog)
            {
                using (var sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\ServiceLogFile.txt", true))
                {
                    sw.WriteLine(DateTime.Now.ToString(DATE_TIME_FORMAT) + ": " + message.Trim());
                }
            }
        }

        public static void WriteException(Exception exception)
        {
            if (ConfigurationHelper.EnableLog)
            {
                using (var sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\ServiceLogFile.txt", true))
                {
                    sw.WriteLine(DateTime.Now.ToString(DATE_TIME_FORMAT) + ": " + exception.Source.ToString().Trim() + "; " + exception.Message.ToString().Trim());
                }
            }
        }
    }
}
