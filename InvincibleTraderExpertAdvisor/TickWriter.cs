using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public class TickWriter : ITickWriter
    {
        private string _accountId;
        private int _currencyPairId;
        private string _registryPath;
        private SQLiteConnection _connection;
        private IUtcClock _utcClock;

        public TickWriter(string registryPath, string accountId, int currencyPairId, IUtcClock utcClock)
        {
            _registryPath = registryPath;
            _accountId = accountId;
            _currencyPairId = currencyPairId;

            _utcClock = utcClock;
        }

        public (bool success, long timestamp, double bid, double ask) LastTick
        {
            get
            {
                var tickFolder = TickUri;

                Directory.CreateDirectory(tickFolder);

                bool success = false;
                long lastTimestamp = long.MinValue;
                double lastBid = double.NaN;
                double lastAsk = double.NaN;

                var tickFiles = Directory.GetFiles(tickFolder);
                if (tickFiles.Length > 0)
                {
                    Array.Sort(tickFiles);
                    Array.Reverse(tickFiles);

                    var builder = new SQLiteConnectionStringBuilder() { DataSource = $@"{tickFiles[0]}" };
                    var connection = new SQLiteConnection(builder.ConnectionString);
                    connection.Open();
                    var query = connection.CreateCommand();
                    query.CommandText = "SELECT timestamp, bid, ask FROM Ticks ORDER BY timestamp DESC LIMIT 1";

                    using (var reader = query.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lastTimestamp = reader.GetInt64(reader.GetOrdinal("timestamp"));
                            lastBid = reader.GetDouble(reader.GetOrdinal("bid"));
                            lastAsk = reader.GetDouble(reader.GetOrdinal("bid"));
                        }
                    }

                    connection.Clone();

                    success = true;
                }

                return (success, lastTimestamp, lastBid, lastAsk);
            }
        }

        public string TickUri 
        {
            get
            {
                return $@"{_registryPath}\{_accountId}\ticks\{_currencyPairId}";
            }
        }

        public (bool success, string message) Write(long timestamp, double bid, double ask)
        {
            bool success = false;
            string message = null;

            var utcTimestamp = _utcClock.FromTicks(timestamp);
            var utcNow = _utcClock.Now;
           
            if (_connection == null || utcTimestamp.Year != utcNow.Year || utcTimestamp.Month != utcNow.Month || utcTimestamp.Day != utcNow.Day)
            {                
                string tickDbFile = $@"{TickUri}\{_currencyPairId}_{utcTimestamp.Year}{utcTimestamp.Month}{utcTimestamp.Day}.tick";

                _connection = OpenTickDb(tickDbFile);
            }

            _connection.Open();

            try
            {                               
                var command = _connection.CreateCommand();

                command.CommandText = TickWriterCommands.Quotes_InsertNewRow;

                command.Parameters.AddWithValue("$timestamp", timestamp);

                command.Parameters.AddWithValue("bid", bid);
                command.Parameters.AddWithValue("ask", ask);

                command.ExecuteNonQuery();

                success = true;
                               
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                _connection.Close();
            }

            return (success, message);
        }

        private SQLiteConnection OpenTickDb(string tickDbFile)
        {
            if (!File.Exists(tickDbFile))
            {
                var tickDbFolder = Path.GetDirectoryName(tickDbFile);
                if (!Directory.Exists(tickDbFolder))
                {
                    Directory.CreateDirectory(tickDbFolder);
                }
            }

            var builder = new SQLiteConnectionStringBuilder() { DataSource = tickDbFile };
            var connection = new SQLiteConnection(builder.ConnectionString);

            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = TickWriterCommands.Quotes_CreateTableIfNotExisting;
            var result = command.ExecuteNonQuery();
            connection.Close();

            return connection;
        }
    }
}
