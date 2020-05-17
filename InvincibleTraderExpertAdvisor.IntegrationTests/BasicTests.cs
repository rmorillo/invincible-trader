using System;
using System.Threading;
using Xunit;

namespace InvincibleTraderExpertAdvisor.IntegrationTests
{
    public class BasicTests
    {
        private readonly string CentralRegistryPath;

        public BasicTests()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                CentralRegistryPath = Environment.GetEnvironmentVariable("INVINCIBLE_TRADER_TEST_REGISTRY_HOME", EnvironmentVariableTarget.Machine);
            }
            else
            {
                CentralRegistryPath = Environment.GetEnvironmentVariable("INVINCIBLE_TRADER_TEST_REGISTRY_HOME");
            }

            Console.WriteLine($"Registry: {CentralRegistryPath}");
        }

        [Fact]
        public void BasicTest()
        {
            var ea = new InvincibleTrader(new UtcClock(), new SQLiteRegistry(CentralRegistryPath), new Beacon(), new Backfiller());

            ea.LogEvent += (logLevel, message) => { Console.WriteLine($"Log Level: {logLevel}, Message: {message}"); };

            ea.Initialize("0d26339b-204c-4a93-a751-47f00b949509", 1, "EURUSD");

            ea.WrapUp();

            Thread.Sleep(1000);
        }
    }
}
