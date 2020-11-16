using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace InvincibleTraderExpertAdvisor
{
    public class BeaconFeeder : IBeaconPort
    {
        private AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        private BackgroundWorker _worker = new BackgroundWorker();

        private PublisherSocket _feederSocket;
        public int PortNumber { get; private set; }

        public bool Started { get; private set; }

        public event Delegates.LogEventHandler LogEvent;

        public ConcurrentQueue<(long tsDateTime, int tsMilliseconds, double bid, double ask)> _quotes;

        public BeaconFeeder()
        {
             _quotes = new ConcurrentQueue<(long tsDateTime, int tsMilliseconds, double bid, double ask)>();

            _worker.DoWork += Worker;
            _worker.WorkerSupportsCancellation = true; ;
        }

        public void Start(int feederPort)
        {
            if (!Started)
            {
                Started = false;
                PortNumber = feederPort;

                if (_worker.IsBusy)
                {
                    while (_worker.IsBusy)
                    {
                        Thread.Sleep(100);
                    }
                }

                _worker.RunWorkerAsync(feederPort);
                _autoResetEvent.WaitOne();
            }
        }

        private void Worker(object sender, DoWorkEventArgs e)
        {
            using (_feederSocket = new PublisherSocket())
            {
                try
                {
                    _feederSocket.Options.SendHighWatermark = 1000;
                    _feederSocket.Bind($"tcp://*:{e.Argument}");
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
                    while (true)
                    {
                        if (_worker.CancellationPending)
                        {
                            e.Cancel = true;
                            break;
                        }

                        while (_quotes.TryDequeue(out (long tsDateTime, int tsMilliseconds, double bid, double ask) result))
                        {
                            _feederSocket.SendMoreFrame("Ticks").SendFrame($"ts:{result.tsDateTime.ToString()}{result.tsMilliseconds.ToString()},bid:{result.bid},ask:{result.ask}");
                        }

                        _autoResetEvent.WaitOne();
                    }
                }
            }
        }

        public void SendTick(long tsDateTime, int tsMilliseconds, double bid, double ask)
        {
            _quotes.Enqueue((tsDateTime, tsMilliseconds, bid, ask));
            _autoResetEvent.Set();
        }

        public void Stop()
        {
            if (_worker.IsBusy)
            {
                _worker.CancelAsync();
                _autoResetEvent.Set();

                while (_worker.IsBusy)
                {
                    Thread.Sleep(100);
                }
            }
        }

        public void Reset()
        {
            Stop();
            Started = false;
            PortNumber = 0;
        }
    }
}
