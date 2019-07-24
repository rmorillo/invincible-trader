using System;

namespace InvincibleTraderExpertAdvisor
{
    public class InvincibleTrader
    {
        private ICentralRegistry _centralRegistry;
        private IBeacon _beacon;
        private int _currencyPairId;        
        private int _sessionId;
        private string _accountId;

        public int LogLevel { get; set; } = 0;

        public event Delegates.LogEventHandler LogEvent;

        public InvincibleTrader(ICentralRegistry centralRegistry, IBeacon beacon)
        {            
            _centralRegistry = centralRegistry;
            _beacon = beacon;
        }

        public void Initialize(string accountId, int sessionId, string currencyPairName)
        {
            _accountId = accountId;
            _sessionId = sessionId;

            ResolveCurrencyPairId(currencyPairName);

            StartBeacon();            

            _centralRegistry.RegisterSession(_accountId, _sessionId, _currencyPairId, _beacon.CommandPortNumber, _beacon.FeederPortNumber);
        }

        public void WrapUp()
        {
            _beacon.Stop();
        }

        ~InvincibleTrader()
        {
            _beacon.Stop();
        }

        public void PriceTick()
        {

        }

        private void ResolveCurrencyPairId(string currencyPairName)
        {
            var result = _centralRegistry.GetCurrencyPairIdByName(currencyPairName);

            if (result.success)
            {
                _currencyPairId = result.currencyPairId;
            }
            else
            {
                throw new Exception("Unable to retrieve currency pair id!");
            }
        }

        private void StartBeacon()
        {           
            var assignedPortNumberResult = _centralRegistry.ReuseCommandPortNumber(_accountId, _sessionId, _currencyPairId);
            int[] availablePortNumbers = null;
            bool hasAvailablePortNumbers = false;

            if (assignedPortNumberResult.success)
            {
                var commandPortNumber = assignedPortNumberResult.portNumber;
                _beacon.Start(commandPortNumber, 8200);
                if (!_beacon.Started)
                {
                    (hasAvailablePortNumbers, availablePortNumbers) = _centralRegistry.GetAvailableCommandPortNumbers(commandPortNumber);
                }
            }
            else
            {
                (hasAvailablePortNumbers, availablePortNumbers) = _centralRegistry.GetAvailableCommandPortNumbers();
            }

            if (!_beacon.Started && !hasAvailablePortNumbers)
            {
                throw new Exception("No available port to open!");
            }

            if (!_beacon.Started && hasAvailablePortNumbers)
            {
                foreach (var portNumber in availablePortNumbers)
                {
                    _beacon.Start(portNumber, 8200);
                    if (_beacon.Started)
                    {
                        break;
                    }
                }
                if (!_beacon.Started)
                {
                    throw new Exception("Unable to start beacon!");
                }
            }            
        }
        
        private void LogMessage(int logLevel, string message)
        {
            if (LogLevel <= logLevel)
            {
                LogEvent?.Invoke(logLevel, message);
            }
        }

    }
}
