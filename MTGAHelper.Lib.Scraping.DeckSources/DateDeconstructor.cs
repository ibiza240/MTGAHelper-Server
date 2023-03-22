using Serilog;
using System;
using System.Text.RegularExpressions;

namespace MTGAHelper.Lib.Scraping.DeckSources
{
    public enum TimespanDurationEnum
    {
        Unknown,
        Milliseconds,
        Seconds,
        Minutes,
        Hours,
        Days,
        Weeks,
        Months,
        Years,
    }

    public interface IDateDeconstructor
    {
        DateTime Deconstruct(string sDateAgo);
    }

    public class DateDeconstructor : IDateDeconstructor
    {
        private readonly Regex regex = new Regex(@"^(.*?) (.*?) ago$", RegexOptions.Compiled);

        public DateTime Deconstruct(string sDateAgo)
        {
            var m = regex.Match(sDateAgo.Trim());

            var nb = Convert.ToInt32(m.Groups[1].Value);
            var unit = GetUnit(m.Groups[2].Value);
            if (unit == TimespanDurationEnum.Unknown)
            {
                Log.Error("Cannot decode string '{sDateAgo}' to date", sDateAgo);
                return default(DateTime);
            }

            var res = GetDecodedDate(nb, unit);
            return res;
        }

        private TimespanDurationEnum GetUnit(string value)
        {
            if (value.StartsWith("ms"))
                return TimespanDurationEnum.Milliseconds;
            if (value.StartsWith("second"))
                return TimespanDurationEnum.Seconds;
            if (value.StartsWith("minute"))
                return TimespanDurationEnum.Minutes;
            if (value.StartsWith("hour"))
                return TimespanDurationEnum.Hours;
            if (value.StartsWith("day"))
                return TimespanDurationEnum.Days;
            if (value.StartsWith("week"))
                return TimespanDurationEnum.Weeks;
            if (value.StartsWith("month"))
                return TimespanDurationEnum.Months;
            if (value.StartsWith("year"))
                return TimespanDurationEnum.Years;

            return TimespanDurationEnum.Unknown;
        }

        private DateTime GetDecodedDate(int nb, TimespanDurationEnum unit)
        {
            switch (unit)
            {
                case TimespanDurationEnum.Milliseconds:
                    return DateTime.UtcNow.Date.AddMilliseconds(-nb);

                case TimespanDurationEnum.Seconds:
                    return DateTime.UtcNow.Date.AddSeconds(-nb);

                case TimespanDurationEnum.Minutes:
                    return DateTime.UtcNow.Date.AddMinutes(-nb);

                case TimespanDurationEnum.Hours:
                    return DateTime.UtcNow.Date.AddHours(-nb);

                case TimespanDurationEnum.Days:
                    return DateTime.UtcNow.Date.AddDays(-nb);

                case TimespanDurationEnum.Weeks:
                    return DateTime.UtcNow.Date.AddDays(-7 * nb);

                case TimespanDurationEnum.Months:
                    return DateTime.UtcNow.Date.AddMonths(-nb);

                case TimespanDurationEnum.Years:
                    return DateTime.UtcNow.Date.AddYears(-nb);
            }

            return DateTime.MinValue;
        }
    }
}