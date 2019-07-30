using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public interface ITickWriter
    {
        (bool success, long timestamp, double bid, double ask) LastTick { get; }
        string TickUri { get; }
        (bool success, string message) Write(long timestamp, double bid, double ask);
    }
}
