using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public interface IBeaconPortAvailability
    {
        (bool success, int[] portNumber) GetAvailableCommandPortNumbers(int exceptThisPortNumber = -1);
        (bool success, int[] portNumber) GetAvailableFeederPortNumbers(int exceptThisPortNumber = -1);
    }
}
