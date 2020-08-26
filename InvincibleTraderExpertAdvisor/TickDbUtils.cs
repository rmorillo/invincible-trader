using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InvincibleTraderExpertAdvisor
{
    public class TickDbUtils
    {        
        public static void CacheMerge(string centralRegistryPath, int currencyPairId, long tsDateTimeStart, long tsDateTimeEnd, IEnumerable<(long tsDateTime, int tsMilliseconds, double bid, double ask)> cachedTicks, int targetSession)
        {

        }

        public static void RegistryMerge(string centralRegistryPath, int currencyPairId, long tsDateTimeStart, long tsDateTimeEnd,  int sourceSession, int targetSession)
        {

        }

        public static void FsmMerge(string centralRegistryPath, int currencyPairId, long tsDateTimeStart, long tsDateTimeEnd, int targetSession)
        {

        }

        public static bool FindBackfillSource()
        {
            return true;
        }
    }
}
