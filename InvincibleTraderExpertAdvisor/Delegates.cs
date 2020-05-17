using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public static class Delegates
    {
        public delegate void LogEventHandler(int logLevel, string message);

        public delegate (long tsDateTime, int tsMilliseconds, double bid, double ask)[] BackfillEventHandler(DateTime startTimestamp, DateTime endTimestamp);

        public delegate (bool success, int[] portNumber) GetAvailablePortNumbersHandler(int exceptThisPortNumber = -1);
    }
}
