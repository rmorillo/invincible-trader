using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public interface IBeacon
    {
        IBeaconPort Commander { get; }
        IBeaconPort Feeder { get; }
        void Start(int commandServerPort, int feederPort, IBeaconPortAvailability portAvailability);
        void Stop();
    }
}
