using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public interface IBackfiller
    {
        event Delegates.BackfillEventHandler BackfillRequest;
        TimeSpan BackfillPeriod { get; }

        void Start(DateTime startTimestamp, DateTime endTimestamp);
        void Stop();
    }
}
