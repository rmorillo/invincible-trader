using System;
using System.Threading;
using Xunit;

namespace InvincibleTraderExpertAdvisor.IntegrationTests
{
    public class BasicTests
    {
        [Fact]
        public void BasicTest()
        {
            var ea = new InvincibleTrader(new SQLiteRegistry(@"D:\InvincibleTrader\registry"), new Beacon());

            ea.LogEvent += (logLevel, message) => { Console.WriteLine($"Log Level: {logLevel}, Message: {message}"); };

            ea.Initialize("0d26339b-204c-4a93-a751-47f00b949509", 1, "EURUSD", new UtcClock(), null, new TimeSpan(0, 1, 0));

            ea.WrapUp();

            Thread.Sleep(1000);
        }
    }
}
