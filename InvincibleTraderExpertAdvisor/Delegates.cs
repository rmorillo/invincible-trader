using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public static class Delegates
    {
        public delegate void LogEventHandler(int logLevel, string message);

        public delegate (long timestamp, double bid, double ask)[] BackfillEventHandler(long startTimestamp, long endTimestamp);
    }
}
