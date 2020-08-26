using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public interface IBeacon
    {
        int CommandPort { get; }
        int FeederPort { get; }
        void Start(int commandServerPort, int feederPort, IBeaconPortAvailability portAvailability);
        void Stop();
        void SendTick(long tsDateTime, int tsMilliseconds, double bid, double ask);
    }
}
