using System;

namespace Poloniex.Core.Utility
{
    public static class TimerUtility
    {
        public static int GetAdjustedInterval(int interval)
        {
            DateTime now = DateTime.UtcNow;
            DateTime next = now.AddSeconds(interval);
            next = next.AddMilliseconds(-next.Millisecond);
            if (interval % 5 == 0)
            {
                if (next.Second % 5 == 9 || next.Second % 5 == 4)
                {
                    next = next.AddSeconds(1);
                }
                else
                if (next.Second % 5 == 8 || next.Second % 5 == 3)
                {
                    next = next.AddSeconds(2);
                }
                else
                if (next.Second % 5 == 1 || next.Second % 5 == 6)
                {
                    next = next.AddSeconds(-1);
                }
                else
                if (next.Second % 5 == 2 || next.Second % 5 == 7)
                {
                    next = next.AddSeconds(-2);
                }
            }
            next = next.AddMilliseconds(5);
            return (int)(next - now).TotalMilliseconds;
        }
    }
}
