using System;

namespace Should.Core
{
    public abstract class DatePrecision
    {
        public static DatePrecision Second = new SecondPrecision();
        public static DatePrecision Minute = new MinutePrecision();
        public static DatePrecision Hour = new HourPrecision();
        public static DatePrecision Date = new DayPrecision();
        public static DatePrecision Month = new MonthPrecision();
        public static DatePrecision Year = new YearPrecision();

        public abstract DateTime Truncate(DateTime date);

        public class SecondPrecision : DatePrecision
        {
            public override DateTime Truncate(DateTime date)
            {
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
            }
        }

        public class MinutePrecision : DatePrecision
        {
            public override DateTime Truncate(DateTime date)
            {
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
            }
        }

        public class HourPrecision : DatePrecision
        {
            public override DateTime Truncate(DateTime date)
            {
                return new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
            }
        }

        public class DayPrecision : DatePrecision
        {
            public override DateTime Truncate(DateTime date)
            {
                return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            }
        }

        public class MonthPrecision : DatePrecision
        {
            public override DateTime Truncate(DateTime date)
            {
                return new DateTime(date.Year, date.Month, 0, 0, 0, 0);
            }
        }

        public class YearPrecision : DatePrecision
        {
            public override DateTime Truncate(DateTime date)
            {
                return new DateTime(date.Year, 0, 0, 0, 0, 0);
            }
        }
    }
}