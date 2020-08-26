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
        private long _currentTimestampDate = 0;
        private string _registryPath;
        private SQLiteConnection _connection;

        public TickWriter(string registryPath, string accountId, int currencyPairId)
        {
            _registryPath = registryPath;
            _accountId = accountId;
            _currencyPairId = currencyPairId;            
        }

        public (bool success, long tsDateTime, int tsMilliseconds, double bid, double ask) LastTick
        {
            get
            {
                var tickFolder = TickDbFolder;

                Directory.CreateDirectory(tickFolder);

                bool success = false;
                long tsDateTime = long.MinValue;
                int tsMilliseconds = int.MinValue;
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
                    query.CommandText = "SELECT tsDateTime, tsMilliseconds, bid, ask FROM Quotes ORDER BY tsDateTime DESC, tsMilliseconds DESC LIMIT 1";

                    using (var reader = query.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            tsDateTime = reader.GetInt64(reader.GetOrdinal("tsDateTime"));
                            tsMilliseconds = reader.GetInt32(reader.GetOrdinal("tsMilliseconds"));
                            lastBid = reader.GetDouble(reader.GetOrdinal("bid"));
                            lastAsk = reader.GetDouble(reader.GetOrdinal("ask"));
                        }
                    }

                    connection.Clone();

                    success = true;
                }

                return (success, tsDateTime, tsMilliseconds, lastBid, lastAsk);
            }
        }

        public string TickDbFolder 
        {
            get
            {
                return $@"{_registryPath}\{_accountId}\ticks\{_currencyPairId}";
            }
        }

        public string TickDbFilename { get; private set; }
        

        public (bool success, string message) Write(long tsDateTime, int tsMilliseconds, double bid, double ask)
        {
            bool success = false;
            string message = null;

            var utcTimestamp = Timestamp.ToDateTime(tsDateTime, tsMilliseconds);

            var tsDate = Timestamp.ExtractTimestampDate(tsDateTime);

            if (_connection == null || tsDate != _currentTimestampDate)
            {          
                if (_connection != null)
                {
                    _connection.Close();
                }

                if (tsDate != _currentTimestampDate)
                {
                    _currentTimestampDate = tsDate;
                }

                TickDbFilename = $@"{_currencyPairId}_{utcTimestamp.Year}{utcTimestamp.Month}{utcTimestamp.Day}.tick";                

                _connection = OpenTickDb(Path.Combine(TickDbFolder, TickDbFilename));
            }

            _connection.Open();

            try
            {                               
                var command = _connection.CreateCommand();

                command.CommandText = TickWriterCommands.Quotes_InsertNewRow;

                command.Parameters.AddWithValue("$tsDateTime", tsDateTime);
                command.Parameters.AddWithValue("$tsMilliseconds", tsMilliseconds);
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
