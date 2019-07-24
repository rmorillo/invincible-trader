using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace InvincibleTraderExpertAdvisor.DbTests
{
    public class SQLiteRegistryTests
    {
        [Fact]
        public void SQLiteRegistryInstantiation_InitializesDbSuccessfully()
        {
            //Arrange
            var (dbFile, path, fileName) = SetupTempDb();

            //Act
            var sqliteReg = new SQLiteRegistry(path, fileName);

            //Assert
            var result = SQLiteDbHelper.Query(dbFile, "SELECT name FROM sqlite_master WHERE type = 'table'");
            Assert.Contains(result, s => s["name"].ToString() == "Sessions");
            Assert.Contains(result, s => s["name"].ToString() == "ReservedPorts");
            var reservedPortsResult = SQLiteDbHelper.Query(dbFile, "SELECT COUNT(*) AS cnt FROM ReservedPorts");
            Assert.Contains(reservedPortsResult, s => (long)(s["cnt"]) > 0);            
            Assert.Contains(result, s => s["name"].ToString() == "CurrencyPairs");
            var currencyPairsResult = SQLiteDbHelper.Query(dbFile, "SELECT COUNT(*) AS cnt FROM CurrencyPairs");
            Assert.Contains(currencyPairsResult, s => (long)(s["cnt"]) > 0);

            TearDownTempDb(dbFile);
        }

        [Fact]
        public void GetCurrencyPairByNameWithExistingName_Succeeds()
        {
            //Arrange
            var (dbFile, path, fileName) = SetupTempDb();

            var sqliteReg = new SQLiteRegistry(path, fileName);
            var randomCurrencyPairName = GetRandomCurrencyPairName(dbFile);

            //Act
            (bool success, string message, int currencyPairId) = sqliteReg.GetCurrencyPairIdByName(randomCurrencyPairName);

            //Assert
            Assert.True(success);
            Assert.True(currencyPairId > 0);

            TearDownTempDb(dbFile);
        }

        [Fact]
        public void GetCurrencyPairByNameWithNoExistingName_Fails()
        {
            //Arrange
            var (dbFile, path, fileName) = SetupTempDb();
            var sqliteReg = new SQLiteRegistry(path, fileName);
            var nonExistingCurrencyPairName = "USDPHP";

            //Act
            (bool success, string message, int currencyPairId) = sqliteReg.GetCurrencyPairIdByName(nonExistingCurrencyPairName);

            //Assert
            Assert.False(success);
            Assert.True(message != null);

            TearDownTempDb(dbFile);
        }

        [Fact]
        public void ReuseCommandPortNumberOfDeadSession_Succeds()
        {
            //Arrange
            var (dbFile, path, fileName) = SetupTempDb();

            var sqliteReg = new SQLiteRegistry(path, fileName);
            var randomCurrencyPairName = GetRandomCurrencyPairName(dbFile);
            (_, _, int currencyPairId) = sqliteReg.GetCurrencyPairIdByName(randomCurrencyPairName);
            (_, int[] commandPortNumbers) = sqliteReg.GetAvailableCommandPortNumbers();
            int randomCommandPortNumber = commandPortNumbers[new Random().Next(0, commandPortNumbers.Length - 1)];
            (_, int[] feederPortNumbers) = sqliteReg.GetAvailableFeederPortNumbers();
            int randomFeederPortNumber = feederPortNumbers[new Random().Next(0, feederPortNumbers.Length - 1)];
            string accountId = Guid.NewGuid().ToString();
            int sessionId = 1;
            sqliteReg.RegisterSession(accountId, sessionId, currencyPairId, randomCommandPortNumber, randomFeederPortNumber);

            //Act
            (bool success, int portNumberResult) = sqliteReg.ReuseCommandPortNumber(accountId, sessionId, currencyPairId);

            //Assert
            Assert.False(success);

            TearDownTempDb(dbFile);
        }

        [Fact]
        public void ReuseCommandPortNumberOfExpiredSession_Succeeds()
        {
            //Arrange
            var (dbFile, path, fileName) = SetupTempDb();

            var sqliteReg = new SQLiteRegistry(path, fileName);
            var randomCurrencyPairName = GetRandomCurrencyPairName(dbFile);
            (_, _, int currencyPairId) = sqliteReg.GetCurrencyPairIdByName(randomCurrencyPairName);
            (_, int[] commandPortNumbers) = sqliteReg.GetAvailableCommandPortNumbers();
            int randomCommandPortNumber = commandPortNumbers[new Random().Next(0, commandPortNumbers.Length - 1)];
            (_, int[] feederPortNumbers) = sqliteReg.GetAvailableFeederPortNumbers();
            int randomFeederPortNumber = feederPortNumbers[new Random().Next(0, feederPortNumbers.Length - 1)];
            string accountId = Guid.NewGuid().ToString();
            int sessionId = 1;
            sqliteReg.RegisterSession(accountId, sessionId, currencyPairId, randomCommandPortNumber, randomFeederPortNumber);

            MarkSessionAsExpired(dbFile, accountId, sessionId, currencyPairId);

            //Act
            (bool success, int portNumberResult) = sqliteReg.ReuseCommandPortNumber(accountId, sessionId, currencyPairId);

            //Assert
            Assert.True(success);
            
            TearDownTempDb(dbFile);
        }

        [Fact]
        public void ReuseFeederPortNumberOfDeadSession_Succeds()
        {
            //Arrange
            var (dbFile, path, fileName) = SetupTempDb();

            var sqliteReg = new SQLiteRegistry(path, fileName);
            var randomCurrencyPairName = GetRandomCurrencyPairName(dbFile);
            (_, _, int currencyPairId) = sqliteReg.GetCurrencyPairIdByName(randomCurrencyPairName);
            (_, int[] commandPortNumbers) = sqliteReg.GetAvailableCommandPortNumbers();
            int randomCommandPortNumber = commandPortNumbers[new Random().Next(0, commandPortNumbers.Length - 1)];
            (_, int[] feederPortNumbers) = sqliteReg.GetAvailableFeederPortNumbers();
            int randomFeederPortNumber = feederPortNumbers[new Random().Next(0, feederPortNumbers.Length - 1)];
            string accountId = Guid.NewGuid().ToString();
            int sessionId = 1;
            sqliteReg.RegisterSession(accountId, sessionId, currencyPairId, randomCommandPortNumber, randomFeederPortNumber);

            //Act
            (bool success, int portNumberResult) = sqliteReg.ReuseCommandPortNumber(accountId, sessionId, currencyPairId);

            //Assert
            Assert.False(success);

            TearDownTempDb(dbFile);
        }

        [Fact]
        public void ReuseFeederPortNumberOfExpiredSession_Succeeds()
        {
            //Arrange
            var (dbFile, path, fileName) = SetupTempDb();

            var sqliteReg = new SQLiteRegistry(path, fileName);
            var randomCurrencyPairName = GetRandomCurrencyPairName(dbFile);
            (_, _, int currencyPairId) = sqliteReg.GetCurrencyPairIdByName(randomCurrencyPairName);
            (_, int[] commandPortNumbers) = sqliteReg.GetAvailableCommandPortNumbers();
            int randomCommandPortNumber = commandPortNumbers[new Random().Next(0, commandPortNumbers.Length - 1)];
            (_, int[] feederPortNumbers) = sqliteReg.GetAvailableFeederPortNumbers();
            int randomFeederPortNumber = feederPortNumbers[new Random().Next(0, feederPortNumbers.Length - 1)];
            string accountId = Guid.NewGuid().ToString();
            int sessionId = 1;
            sqliteReg.RegisterSession(accountId, sessionId, currencyPairId, randomCommandPortNumber, randomFeederPortNumber);

            MarkSessionAsExpired(dbFile, accountId, sessionId, currencyPairId);

            //Act
            (bool success, int portNumberResult) = sqliteReg.ReuseFeederPortNumber(accountId, sessionId, currencyPairId);

            //Assert
            Assert.True(success);

            TearDownTempDb(dbFile);
        }

        private (string dbFile, string path, string fileName) SetupTempDb()
        {
            var dbFile = Path.GetTempFileName();
            
            var path = Path.GetDirectoryName(dbFile);
            var fileName = Path.GetFileName(dbFile);

            return (dbFile, path, fileName);
        }

        private void TearDownTempDb(string dbFile)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.Delete(dbFile);
        }

        private string GetRandomCurrencyPairName(string dbFile)
        {
            var currencyPairs = SQLiteDbHelper.Query(dbFile, "SELECT currencyPairName FROM CurrencyPairs").ToArray();
            return currencyPairs[new Random().Next(0, currencyPairs.Length - 1)]["currencyPairName"].ToString();
        }

        private void MarkSessionAsExpired(string dbFile, string accountId, int sessionId, int currencyPairId)
        {
            string expirationDate = DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 11)).ToString("yyyy-MM-dd HH:mm:ss");

            SQLiteDbHelper.NonQueryCommand(dbFile, $"UPDATE Sessions SET lastKeptAlive = '{expirationDate}' WHERE accountId='{accountId}' AND sessionId={sessionId} AND currencyPairId={currencyPairId}");
        }
    }
}
