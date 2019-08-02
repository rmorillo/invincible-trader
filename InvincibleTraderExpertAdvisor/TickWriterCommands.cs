using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public class TickWriterCommands
    {
        public const string Quotes_CreateTableIfNotExisting =
            "CREATE TABLE IF NOT EXISTS Quotes(tsDateTime INTEGER, tsMilliseconds INTEGER, bid DOUBLE, ask DOUBLE)";

        public const string Quotes_InsertNewRow =
            "INSERT INTO Quotes(tsDateTime, tsMilliseconds, bid, ask) VALUES($tsDateTime, $tsMilliseconds, $bid, $ask)";
    }
}
