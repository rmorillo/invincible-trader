using System;
using System.IO;
using System.Linq;
using Xunit;

namespace InvincibleTraderExpertAdvisor.IntegrationTests
{
    public class TickWriterTests
    {
        private IUtcClock _utcClock = new UtcClock();

        [Fact]
        public void GetTickUriOnNewDbPth_ShouldNotExist()
        {
            //Arrange
            var (dbFile, path, fileName) = SetupTempDb();

            var sqliteReg = new SQLiteRegistry(path, _utcClock, fileName);
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
            var tickUri = tickWriter.TickUri;

            //Assert
            Assert.False(Directory.Exists(tickUri));
        }

        [Fact]
        public void GetLastTickOnEmptyTickFolder_Fails()
        {
            //Arrange
            var (dbFile, path, fileName) = SetupTempDb();

            var sqliteReg = new SQLiteRegistry(path, _utcClock, fileName);
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

            var sqliteReg = new SQLiteRegistry(path, _utcClock, fileName);
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
            var result = tickWriter.Write(DateTime.UtcNow.Ticks, 1, 2);

            //Assert
            Assert.True(result.success);
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
