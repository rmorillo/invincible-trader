using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderHeartbeatPinger
{
    public class SessionInfo
    {
        public int SessionId { get; set; }
        public string AccountId { get; set; }
        public int CurrencyPairId { get; set; }
        public bool IsKeptAlive { get; set; }
        public int KeepAliveStatus { get; set; }
        public int commandPortNumber { get; set; }

    }
}
