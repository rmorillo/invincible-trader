using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public interface ICentralRegistry: IBeaconPortAvailability
    {
        (bool success, string message) RegisterSession(string accountId, int sessionId, int currencyPair, int commandPortNumber, int feederPortNumber);
        (bool success, string message, int currencyPairId) GetCurrencyPairIdByName(string currencyPairName);
        int ReuseCommandPortNumber(string accountId, int sessionId, int currencyPair);        
        int ReuseFeederPortNumber(string accountId, int sessionId, int currencyPair);        
        ITickWriter GetTickWriter(string accountId, int currencyPairId);        
        string Uri { get; }                
    }
}
