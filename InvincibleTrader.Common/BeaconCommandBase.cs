using System;

namespace InvincibleTrader.Common
{
    public abstract class BeaconCommandBase
    {
        protected readonly BeaconCommandOption Command;
        protected readonly int CommandId;
        
        protected byte[] CallPayload;
        private int _callPayloadSize;

        protected byte[] ReturnPayload;
        private int _returnPayloadSize;

        public BeaconCommandBase(BeaconCommandOption command, int callPayloadSize, int returnPayloadSize)
        {
            Command = command;

            _callPayloadSize = callPayloadSize;
            CallPayload = new byte[callPayloadSize];
            Array.Copy(BitConverter.GetBytes((int)command), 0, CallPayload, 0, sizeof(int));

            _returnPayloadSize = returnPayloadSize;
            ReturnPayload = new byte[returnPayloadSize];
            Array.Copy(BitConverter.GetBytes((int)command), 0, ReturnPayload, 0, sizeof(int));
        }

        public bool IsValidCall(byte[] payload)
        {
            return (payload.Length == _callPayloadSize && BitConverter.ToInt32(payload, 0) == (int)Command);            
        }

        public bool IsValidReturn(byte[] payload)
        {
            return (payload.Length == _returnPayloadSize && BitConverter.ToInt32(payload, 0) == (int)Command);
        }
    }
}
