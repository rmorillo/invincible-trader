using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public class SQLiteRegistry: ICentralRegistry
    {
        private SQLiteConnection _connection;

        public SQLiteRegistry(string dbPath, string dbName = "titea_registry.db")
        {
            var builder = new SQLiteConnectionStringBuilder() { DataSource = $@"{dbPath}\{dbName}" };
            _connection = new SQLiteConnection(builder.ConnectionString);
            
            Uri = dbPath;

            ExecuteCommand(SQLiteRegistryCommands.Session_CreateTableIfNotExisting);
            if (ExecuteCommand(SQLiteRegistryCommands.CurrenyPairs_CreateTableIfNotExisting)>=0)
            {
                ExecuteCommand(SQLiteRegistryCommands.CurrencyPairs_InitializeTable);
            }

            if (ExecuteCommand(SQLiteRegistryCommands.ReservedPorts_CreateTableIfNotExisting)>=0)
            {
                ExecuteCommand(SQLiteRegistryCommands.ReservedPorts_InitializeTable);
            }

        }

        public string Uri { get; private set; }        

        public (bool success, string message, int currencyPairId) GetCurrencyPairIdByName(string currencyPairName)
        {
            _connection.Open();

            var selectCommand = _connection.CreateCommand();

            selectCommand.CommandText = "SELECT currencyPairId FROM CurrencyPairs WHERE currencyPairName=$currencyPairName";
            selectCommand.Parameters.AddWithValue("$currencyPairName", currencyPairName);

            int currencyPairId = 0;
            bool success = false;
            string message = null;
            using (var reader = selectCommand.ExecuteReader())
            {
                if (reader.Read())
                {
                    success = true;
                    currencyPairId = reader.GetInt32(0);
                }
                else
                {
                    message = $"Currency Pair with name '{currencyPairName}' doesn't exisit";
                }

                if (reader.Read())
                {
                    message = $"More than 1 Currency Pair names '{currencyPairName}' are found";
                }
            }

            _connection.Close();

            return (success, message, currencyPairId);
        }

        public (bool success, string message) RegisterSession(string accountId, int sessionId, int currencyPair, int commandPortNumber, int feederPortNumber)
        {
            _connection.Open();

            var count = 0;

            using (var query = _connection.CreateCommand())
            {
                query.CommandText = "SELECT COUNT(*) FROM Sessions WHERE currencyPairId=$currencyPairId AND sessionId=$sessionId AND accountId=$accountId";
                query.Parameters.AddWithValue("$currencyPairId", currencyPair);
                query.Parameters.AddWithValue("$sessionId", sessionId);
                query.Parameters.AddWithValue("$accountId", accountId);                

                using (var reader = query.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        count = reader.GetInt32(0);
                    }
                }
            }

            using (var command = _connection.CreateCommand())
            {
                if (count == 0)
                {
                    command.CommandText = SQLiteRegistryCommands.Session_InsertNewRow;
                    
                }
                else
                {
                    command.CommandText = "UPDATE Sessions SET commandPortNumber=$commandPortNumber, feederPortNumber=$feederPortNumber, keepAliveStatus = 0, dtUpdated = $dtUpdated WHERE currencyPairId=$currencyPairId AND sessionId=$sessionId AND accountId=$accountId";
                    command.Parameters.AddWithValue("$dtUpdated", DateTime.Now);                    
                }

                command.Parameters.AddWithValue("$commandPortNumber", commandPortNumber);
                command.Parameters.AddWithValue("$feederPortNumber", feederPortNumber);
                command.Parameters.AddWithValue("$accountId", accountId);
                command.Parameters.AddWithValue("$sessionId", sessionId);
                command.Parameters.AddWithValue("$currencyPairId", currencyPair);

                command.ExecuteNonQuery();
            }
            
            _connection.Close();
            return (true, null);
        }

        public (bool success, int portNumber) ReuseCommandPortNumber(string accountId, int sessionId, int currencyPair)
        {
            _connection.Open();

            var portNumber = 0;

            using (var query = _connection.CreateCommand())
            {
                query.CommandText = SQLiteRegistryCommands.Sessions_QueryCommandPortNumberIfAvailable;
                query.Parameters.AddWithValue("$currencyPairId", currencyPair);
                query.Parameters.AddWithValue("$sessionId", sessionId);
                query.Parameters.AddWithValue("$accountId", accountId);                

                using (var reader = query.ExecuteReader())
                {
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        portNumber = reader.GetInt32(0);
                    }
                }
            }

            _connection.Close();

            return (portNumber > 0, portNumber);
        }

        public (bool success, int[] portNumber) GetAvailableCommandPortNumbers(int exceptThisPortNumber = -1)
        {
            _connection.Open();

            var portNumbers = new List<int>();

            using (var query = _connection.CreateCommand())
            {
                query.CommandText = SQLiteRegistryCommands.Sessions_QueryAvailableCommandPortNumbersWithException;
                query.Parameters.AddWithValue("$exceptThisPortNumber", exceptThisPortNumber);                

                using (var reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        portNumbers.Add(reader.GetInt32(0));
                    }
                }
            }

            _connection.Close();

            return (portNumbers.Count > 0, portNumbers.ToArray());
        }

            private int ExecuteCommand(string commandText)
        {
            _connection.Open();
            var command = _connection.CreateCommand();
            command.CommandText = commandText;
            var result = command.ExecuteNonQuery();            
            _connection.Close();            
            return result;
        }

        public (bool success, int portNumber) ReuseFeederPortNumber(string accountId, int sessionId, int currencyPair)
        {
            _connection.Open();

            var portNumber = 0;

            using (var query = _connection.CreateCommand())
            {
                query.CommandText = SQLiteRegistryCommands.Sessions_QueryFeederPortNumberIfAvailable;
                query.Parameters.AddWithValue("$currencyPairId", currencyPair);
                query.Parameters.AddWithValue("$sessionId", sessionId);
                query.Parameters.AddWithValue("$accountId", accountId);

                using (var reader = query.ExecuteReader())
                {
                    if (reader.Read() && !reader.IsDBNull(0))
                    {
                        portNumber = reader.GetInt32(0);
                    }
                }
            }

            _connection.Close();

            return (portNumber > 0, portNumber);
        }

        public (bool success, int[] portNumber) GetAvailableFeederPortNumbers(int exceptThisPortNumber = -1)
        {
            _connection.Open();

            var portNumbers = new List<int>();

            using (var query = _connection.CreateCommand())
            {
                query.CommandText = SQLiteRegistryCommands.Sessions_QueryAvailableFeederPortNumbersWithException;
                query.Parameters.AddWithValue("$exceptThisPortNumber", exceptThisPortNumber);

                using (var reader = query.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        portNumbers.Add(reader.GetInt32(0));
                    }
                }
            }

            _connection.Close();

            return (portNumbers.Count > 0, portNumbers.ToArray());
        }
    }
}
