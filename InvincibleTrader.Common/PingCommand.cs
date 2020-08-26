using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTrader.Common
{    
    public class PingCommand: BeaconCommandBase
    {
        private byte[] callPayload = new byte[24];

        public PingCommand():base(BeaconCommandOption.Ping, 24, 24)
        {

        }

        public byte[] EncodeCall(long data)
        {
            Array.Copy(BitConverter.GetBytes(data), 0, CallPayload, 4, sizeof(long));
            return CallPayload;
        }

        public (bool success, long data) DecodeCall(byte[] payload)
        {          
            if (IsValidCall(payload))
            {
                return (true, BitConverter.ToInt64(payload, 4));
            }
            else
            {
                return (false, long.MinValue);
            }            
        }

        public byte[] EncodeReturn(long data)
        {
            Array.Copy(BitConverter.GetBytes(data), 0, CallPayload, 4, sizeof(long));
            return CallPayload;
        }

        public (bool success, long data) DecodeReturn(byte[] payload)
        {
            if (IsValidReturn(payload))
            {
                return (true, BitConverter.ToInt64(payload, 4));
            }
            else
            {
                return (false, long.MinValue);
            }
        }
    }
}
