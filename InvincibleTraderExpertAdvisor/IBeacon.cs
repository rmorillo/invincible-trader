using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public interface IBeacon
    {
        int CommandPortNumber { get; }
        int FeederPortNumber { get; }
        event Delegates.LogEventHandler LogEvent;
        void Start(int commandServerPort, int feederPort);
        void Stop();
        bool Started { get; }
    }
}
