using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public interface ICentralRegistry
    {
        (bool success, string message) RegisterSession(string accountId, int sessionId, int currencyPair, int portNumber);
        (bool success, string message, int currencyPairId) GetCurrencyPairIdByName(string currencyPairName);
        (bool success, int portNumber) ReuseCommandPortNumber(string accountId, int sessionId, int currencyPair);
        (bool success, int[] portNumber) GetAvailablePortNumbers(int exceptThisPortNumber = -1);
        string Uri { get; }                
    }
}
