using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public class SQLiteRegistryCommands
    {
        public const string Session_CreateTableIfNotExisting =
            "CREATE TABLE IF NOT EXISTS Sessions(accountId TEXT NOT NULL, sessionId INTEGER NOT NULL, currencyPairId INTEGER NOT NULL, commandPortNumber INTEGER, feederPortNumber INTEGER, isKeptAlive BOOLEAN default 0, lastKeptAlive datetime, keepAliveStatus INTEGER default 0,  dtCreated datetime default current_timestamp, dtUpdated datetime NULL)";

        public const string Session_InsertNewRow =
            "INSERT INTO Sessions(accountId, sessionId, currencyPairId, commandPortNumber, feederPortNumber) VALUES($accountId, $sessionId, $currencyPairId, $commandPortNumber, $feederPortNumber)";

        public const string Sessions_QueryCommandPortNumberIfAvailable =
            "SELECT commandPortNumber FROM Sessions " +
                "WHERE currencyPairId=$currencyPairId AND sessionId=$sessionId AND accountId=$accountId " +
                    "AND (strftime('%s', 'now') - strftime('%s', lastKeptAlive)) > 10";        

        public const string Sessions_QueryAvailableCommandPortNumbersWithException =
            "SELECT portNumber FROM ReservedPorts " +
                "WHERE portType = 1 AND portNumber NOT IN " +
                        "(SELECT commandPortNumber FROM Sessions " +
                            "WHERE (strftime('%s', 'now') - strftime('%s', lastKeptAlive)) < 10 " +
                    "AND portNumber <> $exceptThisPortNumber)";

        public const string Sessions_QueryFeederPortNumberIfAvailable =
            "SELECT feederPortNumber FROM Sessions " +
                "WHERE currencyPairId=$currencyPairId AND sessionId=$sessionId AND accountId=$accountId " +
                    "AND (strftime('%s', 'now') - strftime('%s', lastKeptAlive)) > 10";

        public const string Sessions_QueryAvailableFeederPortNumbersWithException =
            "SELECT portNumber FROM ReservedPorts " +
                "WHERE portType = 2 AND portNumber NOT IN " +
                        "(SELECT feederPortNumber FROM Sessions " +
                            "WHERE (strftime('%s', 'now') - strftime('%s', lastKeptAlive)) < 10 " +
                    "AND portNumber <> $exceptThisPortNumber)";

        public const string CurrenyPairs_CreateTableIfNotExisting = 
            "CREATE TABLE IF NOT EXISTS CurrencyPairs(currencyPairId INTEGER, currencyPairName TEXT)";

        public const string CurrencyPairs_InitializeTable =
            "INSERT INTO CurrencyPairs(currencyPairId, currencyPairName) VALUES" +
                    "(1,'EURUSD')," +
                    "(2,'GBPUSD')," +
                    "(3,'USDJPY')," +
                    "(4,'USDCHF')," +
                    "(5,'AUDUSD')," +
                    "(6,'NZDUSD')," +
                    "(7,'USDCAD')";

        public const string ReservedPorts_CreateTableIfNotExisting =
            "CREATE TABLE IF NOT EXISTS ReservedPorts(portNumber INTEGER NOT NULL, portType INTEGER NOT NULL)";

        public const string ReservedPorts_InitializeTable =
            "INSERT INTO ReservedPorts(portNumber, portType) WITH RECURSIVE " +
                "cntCommandPorts(x) AS " +
                "(" +
                  "SELECT 1 UNION ALL SELECT x+1 FROM cntCommandPorts LIMIT 100" +
                ")," +
                "cntFeederPorts(x) AS " +
                "(" +
                  "SELECT 1 UNION ALL SELECT x+1 FROM cntFeederPorts LIMIT 100" +
                ")" +
                "SELECT 8099 + x, 1 FROM cntCommandPorts " +
                    "UNION ALL " +
                "SELECT 8199 + x, 2 FROM cntFeederPorts";        
    }
}
