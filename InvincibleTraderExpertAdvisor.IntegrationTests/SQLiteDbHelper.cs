using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace InvincibleTraderExpertAdvisor.IntegrationTests
{
    public class SQLiteDbHelper
    {
        public static IEnumerable<Dictionary<string, object>> Query(string dbFile, string sqlQuery)
        {
            var builder = new SQLiteConnectionStringBuilder() { DataSource = dbFile };
            var connection = new SQLiteConnection(builder.ConnectionString);

            connection.Open();

            try
            {
                using (var selectCommand = connection.CreateCommand())
                {
                    ;

                    selectCommand.CommandText = sqlQuery;


                    using (var reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var fields = new Dictionary<string, object>();

                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                fields[reader.GetName(i)] = reader.GetValue(i);
                            }

                            yield return fields;
                        }
                    }
                }
            }
            finally
            {
                connection.Close();
            }            
        }        
    }
}
