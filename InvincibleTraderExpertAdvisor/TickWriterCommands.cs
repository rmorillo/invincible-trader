using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public class TickWriterCommands
    {
        public const string Quotes_CreateTableIfNotExisting =
            "CREATE TABLE IF NOT EXISTS Quotes(timestamp INTEGER, bid DOUBLE, ask DOUBLE)";

        public const string Quotes_InsertNewRow =
            "INSERT INTO Quotes(timestamp, bid, ask) VALUES($timestamp, $bid, $ask)";
    }
}
