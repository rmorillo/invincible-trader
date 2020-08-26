using InvincibleTrader.Common;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InvincibleTraderExpertAdvisor
{
    public class BeaconCommander : IBeaconPort
    {
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        private BackgroundWorker _worker = new BackgroundWorker();

        public int PortNumber { get; private set; }

        public bool Started { get; private set; } = false;      

        public event Delegates.LogEventHandler LogEvent;        

        public BeaconCommander()
        {
            _worker.DoWork += Worker;
            _worker.WorkerSupportsCancellation = true; ;
        }

        public void Start(int commandServerPort)
        {
            if (!Started)
            {
                Started = false;
                PortNumber = commandServerPort;

                if (_worker.IsBusy)
                {
                    while (_worker.IsBusy)
                    {
                        Thread.Sleep(100);
                    }                    
                }

                _worker.RunWorkerAsync(commandServerPort);
                _autoResetEvent.WaitOne();
            }
        }

        public void Stop()
        {
            if (_worker.IsBusy)
            {
                _worker.CancelAsync();

                while (_worker.IsBusy)
                {
                    Thread.Sleep(100);
                }
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
            using (var server = new ResponseSocket())
            {
                try
                {
                    server.Bind($"tcp://*:{e.Argument}");
                    Started = true;
                }
                catch (Exception ex)
                {
                    LogEvent?.Invoke(3, ex.Message);                    
                }
                finally
                {
                    _autoResetEvent.Set();
                }

                if (Started)
                {
                    var pingCommand = new PingCommand();

                    while (true)
                    {
                        if (_worker.CancellationPending)
                        {
                            e.Cancel = true;
                            break;
                        }                        

                        if (server.TryReceiveFrameBytes(TimeSpan.FromSeconds(1), out byte[] payload))
                        {
                            var command = GetCommand(payload);

                            switch(command)
                            {
                                case BeaconCommandOption.Ping:
                                    var (success, data) = pingCommand.DecodeCall(payload);
                                    if (success)
                                    {
                                        server.SendFrame(pingCommand.EncodeReturn(data));
                                    }
                                    break;
                            }
                            //LogEvent?.Invoke(2, $"Received {payload}");
                            //await Task.Delay(100);
                            //LogEvent?.Invoke(2, "Sending World");
                            //server.SendFrame("World");
                            //await Task.Delay(100);
                        }
                    }
                }
            }                        
        }

        private BeaconCommandOption GetCommand(byte[] payload)
        {
            return (BeaconCommandOption)BitConverter.ToInt32(payload, 0);
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
