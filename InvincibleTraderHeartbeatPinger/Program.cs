using InvincibleTrader.Common;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;

namespace InvincibleTraderHeartbeatPinger
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbPath = @"D:\InvincibleTrader\registry";
            var builder = new SQLiteConnectionStringBuilder() { DataSource = $@"{dbPath}\titea_registry.db" };
            var _connection = new SQLiteConnection(builder.ConnectionString);

            while (true)
            {
                var tasks = new List<Task<(bool success, string message, SessionInfo sessionInfo)>>();
                foreach(var row in ReadAllKeepAlivePorts(_connection))
                {
                    tasks.Add(Task.Run(() => Ping(row)));
                }
                Task.WaitAll(tasks.ToArray());

                foreach(var task in tasks)
                {
                    var sessionInfo = task.Result.sessionInfo;

                    Console.WriteLine($"Success: {task.Result.success}, Message: {task.Result.message}, Port: {sessionInfo.commandPortNumber}, Currency Pair Id = {sessionInfo.CurrencyPairId}, Session Id = {sessionInfo.SessionId}, Account  Id = {sessionInfo.AccountId}");
                    
                    _connection.Open();

                    UpdateKeepAliveStatus(_connection, task.Result.success, sessionInfo);

                    _connection.Close();

                }

                Thread.Sleep(5000);
            }

        }

        public static void UpdateKeepAliveStatus(SQLiteConnection connection, bool success, SessionInfo sessionInfo)
        {            
            var command = connection.CreateCommand();                                

            if (success)
            {
                command.CommandText =
                    "UPDATE Sessions SET isKeptAlive=1, keepAliveStatus=1, lastKeptAlive=$lastKeptAlive, dtUpdated=$dtUpdated " +
                        "WHERE accountId=$accountId AND sessionId=$sessionId AND currencyPairId=$currencyPairId";
            }
            else
            {
                command.CommandText = 
                    "UPDATE Sessions SET isKeptAlive=0, keepAliveStatus=$keepAliveStatus, lastKeptAlive=$lastKeptAlive, dtUpdated=$dtUpdated " +
                        "WHERE accountId=$accountId AND sessionId=$sessionId AND currencyPairId=$currencyPairId";

                var nextKeepAliveStatus = GetNewKeepAliveStatus(success, sessionInfo.KeepAliveStatus);
                command.Parameters.AddWithValue("$keepAliveStatus", nextKeepAliveStatus);
            }
            
            
            command.Parameters.AddWithValue("$lastKeptAlive", DateTime.UtcNow);
            command.Parameters.AddWithValue("$dtUpdated", DateTime.UtcNow);
            command.Parameters.AddWithValue("$accountId", sessionInfo.AccountId);
            command.Parameters.AddWithValue("$sessionId", sessionInfo.SessionId);
            command.Parameters.AddWithValue("$currencyPairId", sessionInfo.CurrencyPairId);
            command.ExecuteNonQuery();
        }

        private static int GetNewKeepAliveStatus(bool success, int lastKeepAliveStatus)
        {
            if (success)
            {
                return 1;
            }
            else
            {
                if (lastKeepAliveStatus >= 0)
                    return -1;
                else if (lastKeepAliveStatus == -3)
                    return -3;
                else
                    return lastKeepAliveStatus - 1;
            }
        }

        private async static Task<(bool success, string message, SessionInfo sessionInfo)> Ping(SessionInfo sessionInfo)
        {
            bool success = false;
            string message = null;
            using (var client = new RequestSocket())
            {
                client.Connect($"tcp://localhost:{sessionInfo.commandPortNumber}");

                Console.WriteLine($"Pinging at port {sessionInfo.commandPortNumber}, Currency Pair Id = {sessionInfo.CurrencyPairId}, Session Id = {sessionInfo.SessionId}, Account  Id = {sessionInfo.AccountId}");
                //client.SendFrame("Hello");
                long payload = DateTime.Now.Ticks;
                var pingCommand = new PingCommand();

                client.SendFrame(pingCommand.EncodeCall(payload));
                
                if (client.TryReceiveFrameBytes(new TimeSpan(0, 0, 3), out byte[] reply))
                {
                    message = $"Received reply";
                    success = true;
                }
                else
                {
                    message = "No reply";
                }
            }
            await Task.Delay(100);

            return (success, message, sessionInfo);
        }

        private static SessionInfo[] ReadAllKeepAlivePorts(SQLiteConnection connection)
        {
            var result = new List<SessionInfo>();
            connection.Open();

            var selectCommand = connection.CreateCommand();

            selectCommand.CommandText =
                    "SELECT sessionId, accountId, currencyPairId, isKeptAlive, keepAliveStatus, commandPortNumber FROM Sessions " +
                        "WHERE keepAliveStatus IN (0,-1,-2) " +
                            "OR (isKeptAlive AND (strftime('%s', 'now') - strftime('%s', lastKeptAlive)) >= 5)";            

            using (var reader = selectCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    result.Add(new SessionInfo()
                    {
                        SessionId = reader.GetInt32(reader.GetOrdinal("sessionId")),
                        AccountId = reader.GetString(reader.GetOrdinal("accountId")),
                        CurrencyPairId = reader.GetInt32(reader.GetOrdinal("currencyPairId")),
                        IsKeptAlive = reader.GetBoolean(reader.GetOrdinal("isKeptAlive")),
                        KeepAliveStatus = reader.GetInt32(reader.GetOrdinal("keepAliveStatus")),
                        commandPortNumber = reader.GetInt32(reader.GetOrdinal("commandPortNumber"))
                    });                       
                }
            }

           connection.Close();

            return result.ToArray();
        }

        private void Subscribe(string topic)
        {
            using (var subSocket = new SubscriberSocket())
            {
                subSocket.Options.ReceiveHighWatermark = 1000;
                subSocket.Connect("tcp://localhost:12345");
                subSocket.Subscribe(topic);
                Console.WriteLine("Subscriber socket connecting...");
                while (true)
                {
                    string messageTopicReceived = subSocket.ReceiveFrameString();
                    string messageReceived = subSocket.ReceiveFrameString();
                    Console.WriteLine(messageReceived);
                }
            }
        }
    }
}
