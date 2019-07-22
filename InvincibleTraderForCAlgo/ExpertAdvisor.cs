using System;
using TheInvincibleTraderExpertAdvisor;

namespace TheInvincibleTraderForCAlgo
{
    public class ExpertAdvisor
    {
        private TheInvincibleTrader _ea;

        public void Initialize()
        {
            _ea = new TheInvincibleTrader();
        }
    }
}
