using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public interface ICentralRegistry
    {
        (bool success, string message) RegisterSession(string accountId, int sessionId, int currencyPair, int portNumber);
        (bool success, string message, int currencyPairId) GetCurrencyPairIdByNme(string currencyPairName);
        (bool success, int portNumber) GetAssignedPortNumber(string accountId, int sessionId, int currencyPair);
        (bool success, int[] portNumber) GetAvailablePortNumbers(int exceptThisPortNumber = -1);
        string Uri { get; }                
    }
}
