using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace InvincibleTraderExpertAdvisor.IntegrationTests
{    
    public class TickDbUtilsTests
    {
        private readonly string CentralRegistryPath;

        public TickDbUtilsTests()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                CentralRegistryPath = Environment.GetEnvironmentVariable("INVINCIBLE_TRADER_TEST_REGISTRY_HOME", EnvironmentVariableTarget.Machine);
            }
            else
            {
                CentralRegistryPath = Environment.GetEnvironmentVariable("INVINCIBLE_TRADER_TEST_REGISTRY_HOME");
            }
        }

        [Fact]
        public  void BasicTest()
        {
            //Arrange            
            var tsDateTimeStart = 1;
            var tsDateTimeEnd = 1;
            var cachedTicks = new List<(long, int, double, double)>();


            var (dbFile, path, fileName) = SetupTempDb();
            var sqliteReg = new SQLiteRegistry(path, fileName);
            var randomCurrencyPairName = GetRandomCurrencyPairName(dbFile);
            (_, _, int currencyPairId) = sqliteReg.GetCurrencyPairIdByName(randomCurrencyPairName);
            
            string accountId = Guid.NewGuid().ToString();            
            sqliteReg.RegisterSession(accountId, 1, currencyPairId, 0, 0);
            var tickWriter = sqliteReg.GetTickWriter(accountId, currencyPairId);

            //Act
            var result = tickWriter.LastTick;

            //Act
            TickDbUtils.CacheMerge(CentralRegistryPath, currencyPairId, tsDateTimeStart, tsDateTimeEnd, cachedTicks, 1); ;
            //Assert

            //Run query
            //Assert result
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

        public string GetTemporaryDirectory()
        {
            string tempFolder = Path.GetTempFileName();
            File.Delete(tempFolder);
            Directory.CreateDirectory(tempFolder);

            return tempFolder;
        }
    }
}
