using NetMQ;
using NetMQ.Sockets;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace InvincibleTraderExpertAdvisor
{
    public class Beacon : IBeacon
    {        
        public event Delegates.LogEventHandler LogEvent;

        private BeaconCommander _commander;
        private BeaconFeeder _feeder;

        public int CommandPort { get { return _commander.PortNumber; } }

        public int FeederPort { get { return _feeder.PortNumber; } }

        public Beacon()
        {
            _commander = new BeaconCommander();
            _feeder = new BeaconFeeder();
        }

        public void Start(int commandServerPort, int feederPort, IBeaconPortAvailability portAvailability)
        {            
            StartOnAvailablePort(commandServerPort, _commander, portAvailability.GetAvailableCommandPortNumbers);                        
            StartOnAvailablePort(feederPort, _feeder, portAvailability.GetAvailableFeederPortNumbers);
        }                      

        private void StartOnAvailablePort(int assignedPort, IBeaconPort beaconPort, Delegates.GetAvailablePortNumbersHandler portAvailability)
        {                        
            int[] availablePortNumbers = null;
            bool hasAvailablePortNumbers = false;

            if (assignedPort > 0)
            {
                beaconPort.Start(assignedPort);
                if (!beaconPort.Started)
                {
                    (hasAvailablePortNumbers, availablePortNumbers) = portAvailability(assignedPort);
                }
            }
            else
            {
                (hasAvailablePortNumbers, availablePortNumbers) = portAvailability();
            }

            if (!beaconPort.Started && !hasAvailablePortNumbers)
            {
                throw new Exception("No available port to open!");
            }

            if (!beaconPort.Started && hasAvailablePortNumbers)
            {
                foreach (var portNumber in availablePortNumbers)
                {
                    beaconPort.Reset();
                    beaconPort.Start(portNumber);
                    Thread.Sleep(100);
                    if (beaconPort.Started)
                    {
                        break;
                    }
                }
                if (!beaconPort.Started)
                {
                    throw new Exception("Unable to start beacon!");
                }
            }
        }

        public void Stop()
        {
            _commander.Stop();
            _feeder.Stop();
        }

        public void SendTick(long tsDateTime, int tsMilliseconds, double bid, double ask)
        {
            _feeder.SendTick(tsDateTime, tsMilliseconds, bid, ask);
        }
    }
}
