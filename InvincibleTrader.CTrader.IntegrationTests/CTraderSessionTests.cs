using System;
using Xunit;

namespace InvincibleTrader.CTrader.IntegrationTests
{
    public class CTraderSessionTests
    {
        [Fact]
        public void BasicTest()
        {
            var accountDbPath = @"D:\InvincibleTrader\accounts\ICMarkets.db";
            //Arrange
            var cTrader = new CTraderSession(accountDbPath, "Invincible Trader- Demo 3779328 Main Session(1), Version = 1.0.0.0, Culture = neutral, PublicKeyToken = null", "1.0.0.0", 3779328, false);
        }
    }
}
