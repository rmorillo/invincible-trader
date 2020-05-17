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
        private IUtcClock _utcClock;
        private ITickWriter _tickWriter;
        private IBackfiller _backfiller;

        public int LogLevel { get; set; } = 0;

        public event Delegates.LogEventHandler LogEvent;

        public InvincibleTrader(IUtcClock utcClock, ICentralRegistry centralRegistry, IBeacon beacon, IBackfiller backfiller)
        {
            _utcClock = utcClock;
            _centralRegistry = centralRegistry;
            _beacon = beacon;
            _backfiller = backfiller;
        }

        public void Initialize(string accountId, int sessionId, string currencyPairName)
        {
            _accountId = accountId;
            _sessionId = sessionId;

            ResolveCurrencyPairId(currencyPairName);            
                        
            _centralRegistry.RegisterSession(_accountId, _sessionId, _currencyPairId, _beacon.Commander.PortNumber, _beacon.Feeder.PortNumber);            

            StartBeacon();            

            _tickWriter = _centralRegistry.GetTickWriter(_accountId, _currencyPairId);

            StartBackfill();
        }

        public void StartBackfill()
        {
            var (success, tsDateTime, tsMilliseconds, bid, ask) = _tickWriter.LastTick;

            var endTime = _utcClock.Now;
            var startTime = endTime;

            if (success)
            {
                startTime = Timestamp.ToDateTime(tsDateTime, tsMilliseconds);
            }
            else
            {
                startTime = endTime.Subtract(_backfiller.BackfillPeriod);
            }

            _backfiller.Start(startTime, endTime);
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
            var (success, message, currencyPairId) = _centralRegistry.GetCurrencyPairIdByName(currencyPairName);

            if (success)
            {
                _currencyPairId = currencyPairId;
            }
            else
            {
                throw new Exception("Unable to retrieve currency pair id!");
            }
        }

        private void StartBeacon()
        {
            var assignedCommandPortNumber = _centralRegistry.ReuseCommandPortNumber(_accountId, _sessionId, _currencyPairId);
            var assignedFeederPortNumber = _centralRegistry.ReuseFeederPortNumber(_accountId, _sessionId, _currencyPairId);

            _beacon.Start(assignedCommandPortNumber, assignedFeederPortNumber, _centralRegistry);            
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
