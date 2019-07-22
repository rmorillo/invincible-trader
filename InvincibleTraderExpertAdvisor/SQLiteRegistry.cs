using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public class SQLiteRegistry: ICentralRegistry
    {
        private SQLiteConnection _connection;

        public SQLiteRegistry(string dbPath)
        {
            var builder = new SQLiteConnectionStringBuilder() { DataSource = $@"{dbPath}\titea_registry.db" };
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

        public (bool success, string message, int currencyPairId) GetCurrencyPairIdByNme(string currencyPairName)
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
                    message = $"Currency Pair Id '{currencyPairId}' doesn't exisit";
                }

                if (reader.Read())
                {
                    message = $"More than 1 Currency Pair Id '{currencyPairId}' are found";
                }
            }

            _connection.Close();

            return (success, message, currencyPairId);
        }

        public (bool success, string message) RegisterSession(string accountId, int sessionId, int currencyPair, int portNumber)
        {
            _connection.Open();

            var query = _connection.CreateCommand();
            query.CommandText = "SELECT COUNT(*) FROM Sessions WHERE currencyPairId=$currencyPairId AND sessionId=$sessionId AND accountId=$accountId";
            query.Parameters.AddWithValue("$currencyPairId", currencyPair);
            query.Parameters.AddWithValue("$sessionId", sessionId);
            query.Parameters.AddWithValue("$accountId", accountId);

            var count = 0;

            using (var reader = query.ExecuteReader())
            {
                if (reader.Read())
                {
                    count = reader.GetInt32(0);
                }             
            }

            var command = _connection.CreateCommand();

            if (count == 0)
            {                
                command.CommandText = SQLiteRegistryCommands.Session_InsertNewRow;
                command.Parameters.AddWithValue("$commandPortNumber", portNumber);
                command.Parameters.AddWithValue("$accountId", accountId);
                command.Parameters.AddWithValue("$sessionId", sessionId);
                command.Parameters.AddWithValue("$currencyPairId", currencyPair);                
            }
            else
            {                
                command.CommandText = "UPDATE Sessions SET commandPortNumber=$commandPortNumber, keepAliveStatus = 0, dtUpdated = $dtUpdated WHERE currencyPairId=$currencyPairId AND sessionId=$sessionId";
                command.Parameters.AddWithValue("$commandPortNumber", portNumber);
                command.Parameters.AddWithValue("$dtUpdated", DateTime.Now);
                command.Parameters.AddWithValue("$sessionId", sessionId);
                command.Parameters.AddWithValue("$currencyPairId", currencyPair);
                
            }

            command.ExecuteNonQuery();            
            _connection.Close();
            return (true, null);
        }

        public (bool success, int portNumber) GetAssignedPortNumber(string accountId, int sessionId, int currencyPair)
        {
            _connection.Open();

            var query = _connection.CreateCommand();
            query.CommandText = SQLiteRegistryCommands.Sessions_QueryAssignedPortNumberIfAvailable;
            query.Parameters.AddWithValue("$currencyPairId", currencyPair);
            query.Parameters.AddWithValue("$sessionId", sessionId);
            query.Parameters.AddWithValue("$accountId", accountId);

            var portNumber = 0;

            using (var reader = query.ExecuteReader())
            {
                if (reader.Read())
                {
                    portNumber = reader.GetInt32(0);
                }
            }

            _connection.Close();

            return (portNumber > 0, portNumber);
        }

        public (bool success, int[] portNumber) GetAvailablePortNumbers(int exceptThisPortNumber = -1)
        {
            _connection.Open();

            var query = _connection.CreateCommand();
            query.CommandText = SQLiteRegistryCommands.Sessions_QueryAvailablePortNumbersWithException;
            query.Parameters.AddWithValue("$exceptThisPortNumber", exceptThisPortNumber);

            var portNumbers = new List<int>();

            using (var reader = query.ExecuteReader())
            {
                while (reader.Read())
                {
                    portNumbers.Add(reader.GetInt32(0));
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
               
    }
}
