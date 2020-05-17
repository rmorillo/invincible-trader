using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public class BeaconFeeder : IBeaconPort
    {
        private PublisherSocket _feederSocket;
        public int PortNumber => throw new NotImplementedException();

        public bool Started => throw new NotImplementedException();

        public event Delegates.LogEventHandler LogEvent;

        public void Start(int feederPort)
        {           
            Random rand = new Random(50);
            _feederSocket = new PublisherSocket();

            _feederSocket.Options.SendHighWatermark = 1000;
            _feederSocket.Bind($"tcp://*:{feederPort}");
            /*
            for (var i = 0; i < 100; i++)
            {
                var randomizedTopic = rand.NextDouble();
                if (randomizedTopic > 0.5)
                {
                    var msg = "TopicA msg-" + i;
                    Console.WriteLine("Sending message : {0}", msg);
                    _feederSocket.SendMoreFrame("TopicA").SendFrame(msg);
                }
                else
                {
                    var msg = "TopicB msg-" + i;
                    Console.WriteLine("Sending message : {0}", msg);
                    _feederSocket.SendMoreFrame("TopicB").SendFrame(msg);
                }
                Thread.Sleep(500);

            }
            */
        }

        public void Stop()
        {
            if (!_feederSocket.IsDisposed)
            {
                _feederSocket.Close();
                _feederSocket.Dispose();
            }
        }
    }
}
