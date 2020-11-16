using System;
using Xunit;
using InvincibleTraderExpertAdvisor;

namespace InvincibleTraderExpertAdvisor.UnitTests
{
    public class BasicTests
    {
        [Fact]
        public void BasicTest()
        {
            var ea = new InvincibleTraderSession(null, null, null, null, null);
        }
    }
}
