using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public interface IUtcClock
    {
        DateTime Now { get; }
        DateTime FromTicks(long ticks);
    }
}
