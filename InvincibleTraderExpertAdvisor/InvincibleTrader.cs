using System;

namespace InvincibleTraderExpertAdvisor
{
    public class InvincibleTraderSession
    {
        private ICentralRegistry _centralRegistry;
        private IBeacon _beacon;
        private int _currencyPairId;        
        private int _sessionId;
        private string _accountId;
        private IUtcClock _utcClock;
        private ITickWriter _tickWriter;
        private IBackfiller _backfiller;
        private ILogger _logger;

        public int LogLevel { get; set; } = 0;

        public event Delegates.LogEventHandler LogEvent;

        public InvincibleTraderSession(IUtcClock utcClock, ICentralRegistry centralRegistry, IBeacon beacon, IBackfiller backfiller, ILogger logger)
        {
            _utcClock = utcClock;
            _centralRegistry = centralRegistry;
            _beacon = beacon;
            _backfiller = backfiller;
            _logger = logger;
        }

        public void Initialize(string accountId, int sessionId, string currencyPairName)
        {
            _accountId = accountId;
            _sessionId = sessionId;

            ResolveCurrencyPairId(currencyPairName);

            var assignedCommandPortNumber = _centralRegistry.ReuseCommandPortNumber(_accountId, _sessionId, _currencyPairId);
            var assignedFeederPortNumber = _centralRegistry.ReuseFeederPortNumber(_accountId, _sessionId, _currencyPairId);

            _beacon.Start(assignedCommandPortNumber, assignedFeederPortNumber, _centralRegistry);

            _centralRegistry.RegisterSession(_accountId, _sessionId, _currencyPairId, _beacon.CommandPort, _beacon.FeederPort);            

            _tickWriter = _centralRegistry.GetTickWriter(_accountId, _currencyPairId);

            StartBackfill();
        }

        public void StartBackfill()
        {
            var (success, tsDateTime, tsMilliseconds, _, _) = _tickWriter.LastTick;

            var endTime = _utcClock.Now;

            DateTime startTime;

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

        ~InvincibleTraderSession()
        {
            _beacon.Stop();
        }

        public void PriceTick(long tsDateTime, int tsMilliseconds, double bid, double ask)
        {
            _tickWriter.Write(tsDateTime, tsMilliseconds, bid, ask);
            _beacon.SendTick(tsDateTime, tsMilliseconds, bid, ask);
        }

        public int PublisherPort {  get { return _beacon.FeederPort; } }

        private void ResolveCurrencyPairId(string currencyPairName)
        {
            var (success, _, currencyPairId) = _centralRegistry.GetCurrencyPairIdByName(currencyPairName);

            if (success)
            {
                _currencyPairId = currencyPairId;
            }
            else
            {
                throw new Exception("Unable to retrieve currency pair id!");
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
