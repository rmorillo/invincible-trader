using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace InvincibleTraderExpertAdvisor
{
    public class BeaconCommander : IBeaconPort
    {
        private BackgroundWorker _worker = new BackgroundWorker();

        public int PortNumber { get; private set; }

        public bool Started => throw new NotImplementedException();

        public event Delegates.LogEventHandler LogEvent;

        public void Start(int commandServerPort)
        {
            PortNumber = commandServerPort;
            _worker.DoWork += Worker;
            _worker.WorkerSupportsCancellation = true; ;
            _worker.RunWorkerAsync(commandServerPort);
        }

        public void Stop()
        {
            if (_worker.IsBusy && !_worker.CancellationPending)
            {
                _worker.CancelAsync();
                Ping();
            }
        }

        private void Ping()
        {
            string message = null;
            using (var client = new RequestSocket())
            {
                client.Connect($"tcp://localhost:{PortNumber}");
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
