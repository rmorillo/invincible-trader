using NetMQ;
using NetMQ.Sockets;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace InvincibleTraderExpertAdvisor
{
    public class Beacon : IBeacon
    {
        public bool Started { get; private set; } = false;
        public int CommandPortNumber { get; private set; } = -1;
        public int FeederPortNumber { get; private set; } = -1;

        public event Delegates.LogEventHandler LogEvent;

        private BackgroundWorker _worker= new BackgroundWorker();

        private PublisherSocket _feederSocket;

        public void Start(int commandServerPort, int feederPort)
        {
            CommandPortNumber = commandServerPort;
            StartCommandServer(commandServerPort);

            FeederPortNumber = feederPort;
            StartFeedPublisher(feederPort);

            Started = true;                       
        }

        private void StartCommandServer(int portNumber)
        {
            _worker.DoWork += Worker;
            _worker.WorkerSupportsCancellation = true; ;
            _worker.RunWorkerAsync(portNumber);
        }

        public void Stop()
        {
            if (_worker.IsBusy && ! _worker.CancellationPending)
            {
                _worker.CancelAsync();
                Ping();
            }
            
            if (!_feederSocket.IsDisposed)
            {
                _feederSocket.Close();
                _feederSocket.Dispose();
            }                                    
        }

        private void StartFeedPublisher(int portNumber)
        {
            Random rand = new Random(50);
            _feederSocket = new PublisherSocket();
                            
            _feederSocket.Options.SendHighWatermark = 1000;
            _feederSocket.Bind($"tcp://*:{portNumber}");
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

        private void Ping()
        {
            string message = null;
            using (var client = new RequestSocket())
            {
                client.Connect($"tcp://localhost:{CommandPortNumber}");                
                client.SendFrame("Hello");
                if (client.TryReceiveFrameString(new TimeSpan(0, 0, 3), out string reply))
                {
                    message = $"Received {reply}";
                }
                else
                {
                    message = "No reply";
                }
            }           
        }

        private void Worker(object sender, DoWorkEventArgs e)
        {
            try
            {
                using (var server = new ResponseSocket())
                {
                    server.Bind($"tcp://*:{e.Argument}");

                    while (true)
                    {
                        if (_worker.CancellationPending)
                        {
                            e.Cancel = true;
                            break;
                        }

                        var message = server.ReceiveFrameString();
                        LogEvent?.Invoke(2, $"Received {message}");
                        //await Task.Delay(100);
                        LogEvent?.Invoke(2, "Sending World");
                        server.SendFrame("World");                        
                        //await Task.Delay(100);
                    }                    
                }

            }
            catch (Exception ex)
            {
                LogEvent?.Invoke(3, ex.Message);
            }            
        }

        private static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1)
        {
            var waitTask = Task.Run(async () =>
            {
                while (!condition()) await Task.Delay(frequency);
            });

            if (waitTask != await Task.WhenAny(waitTask,
                    Task.Delay(timeout)))
                throw new TimeoutException();
        }
    }
}
