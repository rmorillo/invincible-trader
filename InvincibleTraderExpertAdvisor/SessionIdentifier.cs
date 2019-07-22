using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace InvincibleTraderExpertAdvisor
{
    public class SessionIdentifier : ISessionIdentifier
    {
        public int GetSessionId()
        {
            return Process.GetCurrentProcess().Id;
        }
    }
}
