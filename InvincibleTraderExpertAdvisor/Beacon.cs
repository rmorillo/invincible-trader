using NetMQ;
using NetMQ.Sockets;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace InvincibleTraderExpertAdvisor
{
    public class Beacon : IBeacon
    {        
        public event Delegates.LogEventHandler LogEvent;        

        private PublisherSocket _feederSocket;

        public IBeaconPort Commander { get; private set; }

        public IBeaconPort Feeder { get; private set; }

        public void Start(int commandServerPort, int feederPort, IBeaconPortAvailability portAvailability)
        {
            Commander = new BeaconCommander();
            StartListening(commandServerPort, Commander, portAvailability.GetAvailableCommandPortNumbers);            

            Feeder = new BeaconFeeder();
            StartListening(feederPort, Feeder, portAvailability.GetAvailableFeederPortNumbers);

        }                      

        private void StartListening(int assignedPort, IBeaconPort beaconPort, Delegates.GetAvailablePortNumbersHandler portAvailability)
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
                    beaconPort.Start(portNumber);
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
            Commander.Stop();
            Feeder.Stop();
        }
    }
}
