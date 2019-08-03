using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public class Backfiller : IBackfiller
    {
        public TimeSpan BackfillPeriod { get; }

        public event Delegates.BackfillEventHandler BackfillRequest;

        private BackgroundWorker _worker = new BackgroundWorker();

        public void Start(DateTime startTimestamp, DateTime endTimestamp)
        {
            _worker.DoWork += Worker;
            _worker.WorkerSupportsCancellation = true; ;
            _worker.RunWorkerAsync((startTimestamp, endTimestamp));
        }

        private void Worker(object sender, DoWorkEventArgs e)
        {
            var (startTimestamp, endTimestamp) = ((DateTime, DateTime)) e.Argument;
            BackfillRequest?.Invoke(startTimestamp, endTimestamp);
        }

        public void Stop()
        {

        }
    }
}
