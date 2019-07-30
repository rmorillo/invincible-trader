using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public interface ICentralRegistry
    {
        (bool success, string message) RegisterSession(string accountId, int sessionId, int currencyPair, int commandPortNumber, int feederPortNumber);
        (bool success, string message, int currencyPairId) GetCurrencyPairIdByName(string currencyPairName);
        (bool success, int portNumber) ReuseCommandPortNumber(string accountId, int sessionId, int currencyPair);
        (bool success, int[] portNumber) GetAvailableCommandPortNumbers(int exceptThisPortNumber = -1);
        (bool success, int portNumber) ReuseFeederPortNumber(string accountId, int sessionId, int currencyPair);
        (bool success, int[] portNumber) GetAvailableFeederPortNumbers(int exceptThisPortNumber = -1);
        ITickWriter GetTickWriter(string accountId, int currencyPairId);        
        string Uri { get; }                
    }
}
