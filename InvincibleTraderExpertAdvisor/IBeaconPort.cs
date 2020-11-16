using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public interface IBeaconPort
    {
        int PortNumber { get; }        
        event Delegates.LogEventHandler LogEvent;
        void Start(int commandServerPort);        
        void Stop();

        void Reset();
        bool Started { get; }
    }
}
