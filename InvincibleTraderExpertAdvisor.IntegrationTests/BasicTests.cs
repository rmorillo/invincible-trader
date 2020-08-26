using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace InvincibleTraderExpertAdvisor.IntegrationTests
{
    public class BasicTests
    {
        private readonly string CentralRegistryPath;

        public BasicTests()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                CentralRegistryPath = Environment.GetEnvironmentVariable("INVINCIBLE_TRADER_TEST_REGISTRY_HOME", EnvironmentVariableTarget.Machine);
            }
            else
            {
                CentralRegistryPath = Environment.GetEnvironmentVariable("INVINCIBLE_TRADER_TEST_REGISTRY_HOME");
            }

            Console.WriteLine($"Registry: {CentralRegistryPath}");
        }

        [Fact]
        public void BasicFeederTest()
        {
            var ea = new InvincibleTraderSession(new UtcClock(), new SQLiteRegistry(CentralRegistryPath), new Beacon(), new Backfiller());

            ea.LogEvent += (logLevel, message) => { Console.WriteLine($"Log Level: {logLevel}, Message: {message}"); };

            ea.Initialize("0d26339b-204c-4a93-a751-47f00b949509", 1, "EURUSD");

            var tickFeed = new Stack<string>();

            var autoResetEvent = new AutoResetEvent(false);
            var t = new Task(() =>
            {                
                using (var subSocket = new SubscriberSocket())
                {
                    subSocket.Options.ReceiveHighWatermark = 1000;
                    subSocket.Connect($"tcp://localhost:{ea.PublisherPort}");
                    subSocket.Subscribe("Ticks");

                    autoResetEvent.Set();
                    string messageTopicReceived = subSocket.ReceiveFrameString();
                    string messageReceived = subSocket.ReceiveFrameString();
                    tickFeed.Push(messageReceived);                                        
                }         
            });

            t.Start();

            autoResetEvent.WaitOne();

            Thread.Sleep(1000);

            var (tsDateTime, tsMilliseconds) = Timestamp.FromDateTime(DateTime.Now);

            ea.PriceTick(tsDateTime, tsMilliseconds, 1.1234, 1.12345);            

            t.Wait(1000);

            ea.WrapUp();
        }

        [Fact]
        public void MultiCurrencyPairSession()
        {
            var firstCurrency = new InvincibleTraderSession(new UtcClock(), new SQLiteRegistry(CentralRegistryPath), new Beacon(), new Backfiller());
            firstCurrency.Initialize("0d26339b-204c-4a93-a751-47f00b949509", 1, "EURUSD");

            var secondCurrency = new InvincibleTraderSession(new UtcClock(), new SQLiteRegistry(CentralRegistryPath), new Beacon(), new Backfiller());
            secondCurrency.Initialize("0d26339b-204c-4a93-a751-47f00b949509", 1, "GBPUSD");

            firstCurrency.WrapUp();
            secondCurrency.WrapUp();
        }
    }
}
