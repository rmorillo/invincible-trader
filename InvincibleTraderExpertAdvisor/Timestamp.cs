using System;
using System.Collections.Generic;
using System.Text;

namespace InvincibleTraderExpertAdvisor
{
    public class Timestamp
    {
        public static (long dateTime, int milliseconds) FromDateTime(DateTime fromDateTime)
        {
            return (GetTimestampDateTime(fromDateTime), fromDateTime.Millisecond);
        }

        public static DateTime ToDateTime(long timestampDateTime, int timestampMilliseconds)
        {
            int yearPart = (int)(timestampDateTime / 10000000000);
            int yearMonthPart = (int)(timestampDateTime / 100000000);
            int yearMonthDayPart = (int)(timestampDateTime / 1000000);
            int hourMinSecPart = (int)(timestampDateTime -(yearMonthDayPart * 1000000L));
            int hourPart = hourMinSecPart / 10000;
            int hourMinPart = hourMinSecPart / 100;
            return new DateTime(yearPart, yearMonthPart - (yearPart * 100), yearMonthDayPart - (yearMonthPart * 100), hourPart, hourMinPart - (hourPart * 100), hourMinSecPart - (hourMinPart * 100), timestampMilliseconds);
        }

        public static long ExtractTimestampDate(long timestampDateTime)
        {
            return (timestampDateTime / 1000000);            
        }

        private static long GetTimestampDateTime(DateTime dateTime)
        {            
            return (dateTime.Year * 10000000000) + (dateTime.Month * 100000000) + (dateTime.Day * 1000000) + (dateTime.Hour * 10000) + (dateTime.Minute * 100) + (dateTime.Second);
        }
    }
}
