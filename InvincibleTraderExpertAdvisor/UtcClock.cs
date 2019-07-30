using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public class UtcClock : IUtcClock
    {
        public DateTime Now => DateTime.UtcNow;

        public DateTime FromTicks(long ticks)
        {
            return new DateTime(ticks).ToUniversalTime();
        }
    }
}
