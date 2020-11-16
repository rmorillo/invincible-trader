using InvincibleTrader.Common;
using InvincibleTraderExpertAdvisor;
using System;

namespace InvincibleTrader.CTrader
{
    public class CTraderSession
    {
        private const string CentralRegistryPathEnvironmentVariable = "INVINCIBLE_TRADER_CENTRAL_REGISTRY_PATH";
        private const string AccountId = "0d26339b-204c-4a93-a751-47f00b949509";
        private readonly string _centralRegistryPath;
        private const int SessionId = 1;

        private Action _stopInstance;

        private InvincibleTraderSession _expertAdvisor;

        public CTraderSession(string accountDbPath, string cTraderAssemblyFullName, string cTraderAssemblyVersion, int cTraderAccountNumber, bool isLive)
        {
            _centralRegistryPath = Environment.GetEnvironmentVariable(CentralRegistryPathEnvironmentVariable, EnvironmentVariableTarget.Machine);

            var strategyName = cTraderAssemblyFullName.Substring(0, cTraderAssemblyFullName.IndexOf(',') - 1);

            var (executionType, accountNumber, _, _) = ParseStrategyName(strategyName);

            if (cTraderAccountNumber != accountNumber)
            {
                throw new Exception("Strategy Account number mismatch");
            }

            if (isLive)
            {
                if (executionType == ExecutionType.Demo)
                {
                    throw new Exception("Expected to run via Live account");
                }
            }
            else
            {
                if (executionType == ExecutionType.Live)
                {
                    throw new Exception("Expected to run via Demo account");
                }
            }
        }

        private (ExecutionType executionType, int accountNumber, SessionType sessionType, int sessionId) ParseStrategyName(string strategyName)
        {
            string[] split = strategyName.Split(' ');
            var executionType = (ExecutionType)Enum.Parse(typeof(ExecutionType), split[2]);
            var accountNumber = int.Parse(split[3]);
            var sessionType = (SessionType)Enum.Parse(typeof(SessionType), split[4]);
            var sessionId = int.Parse(split[5].Replace("Session(", "").Replace(")", ""));
            return (executionType, accountNumber, sessionType, sessionId);
        }

        public CTraderSession SetStopInstanceHandler(Action stopInstance)
        {
            _stopInstance = stopInstance;
            return this;
        }

        public void Start(string currencyPairName)
        {            
            _expertAdvisor = new InvincibleTraderSession(new UtcClock(), new SQLiteRegistry(_centralRegistryPath), new Beacon(), new Backfiller(), null);
            _expertAdvisor.Initialize(AccountId, SessionId, currencyPairName);

        }

        public void Stop()
        {
            _expertAdvisor.WrapUp();
        }

        public void Tick(DateTime timestampInUtc, double bid, double ask)
        {
            var timestamp = Timestamp.FromDateTime(timestampInUtc);
            _expertAdvisor.PriceTick(timestamp.Item1, timestamp.Item2, bid, ask);
        }
    }
}
