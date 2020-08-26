using System;
using System.IO;
using System.Linq;
using Xunit;

namespace InvincibleTraderExpertAdvisor.IntegrationTests
{
    public class TickWriterTests
    {
        [Fact]
        public void GetTickUriOnNewDbPth_ShouldNotExist()
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
            var tickWriter = sqliteReg.GetTickWriter(accountId, currencyPairId);

            //Act
            var tickUri = tickWriter.TickDbFolder;

            //Assert
            Assert.False(Directory.Exists(tickUri));
        }

        [Fact]
        public void GetLastTickOnEmptyTickFolder_Fails()
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
            var tickWriter = sqliteReg.GetTickWriter(accountId, currencyPairId);

            //Act
            var result = tickWriter.LastTick;

            //Assert
            Assert.False(result.success);
        }

        [Fact]
        public void WriteTick_Succeeds()
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
            var tickWriter = sqliteReg.GetTickWriter(accountId, currencyPairId);
            var timestamp = Timestamp.FromDateTime(DateTime.UtcNow);

            //Act
            var result = tickWriter.Write(timestamp.dateTime, timestamp.milliseconds, 1, 2);

            //Assert
            Assert.True(result.success);
        }

        [Fact]
        public void Write_CreatesNewDbFileWhenDateChanges()
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
            var tickWriter = sqliteReg.GetTickWriter(accountId, currencyPairId);
            var currentDateTime = DateTime.UtcNow;
            var timestamp = Timestamp.FromDateTime(currentDateTime);
            var nextDayTimestamp = Timestamp.FromDateTime(currentDateTime.AddDays(1));
            
            tickWriter.Write(timestamp.dateTime, timestamp.milliseconds, 1, 2);
            var tickDbFilename = tickWriter.TickDbFilename;

            //Act
            var result = tickWriter.Write(nextDayTimestamp.dateTime, nextDayTimestamp.milliseconds, 1, 2);

            //Assert
            Assert.True(result.success);
            Assert.True(tickWriter.TickDbFilename != tickDbFilename);
        }

        private (string dbFile, string path, string fileName) SetupTempDb()
        {
            var dbFile = Path.GetTempFileName();

            var path = Path.GetDirectoryName(dbFile);
            var fileName = Path.GetFileName(dbFile);

            return (dbFile, path, fileName);
        }

        private string GetRandomCurrencyPairName(string dbFile)
        {
            var currencyPairs = SQLiteDbHelper.Query(dbFile, "SELECT currencyPairName FROM CurrencyPairs").ToArray();
            return currencyPairs[new Random().Next(0, currencyPairs.Length - 1)]["currencyPairName"].ToString();
        }
    }
}
