using System;
using Xunit;
using InvincibleTraderExpertAdvisor;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace InvincibleTraderExpertAdvisor.DbTests
{
    public class BasicTests
    {
        [Fact]
        public void BasicTest()
        {
            var ea = new InvincibleTrader(new SQLiteRegistry(@"D:\InvincibleTrader\registry"), new Beacon());

            ea.LogEvent += (logLevel, message) => { Console.WriteLine($"Log Level: {logLevel}, Message: {message}"); };

            ea.Initialize("0d26339b-204c-4a93-a751-47f00b949509", 1, "EURUSD");

            ea.WrapUp();

            Thread.Sleep(60000);            
        }
    }
}
