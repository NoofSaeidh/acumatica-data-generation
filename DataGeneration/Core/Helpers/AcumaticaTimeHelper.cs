using System;

namespace DataGeneration.Core.Helpers
{
    public static class AcumaticaTimeHelper
    {
        public static string FromMinutes(int minutes)
        {
            var hours = minutes / 60;
            minutes = minutes % 60;
            return FromTime(hours, minutes);
        }

        // time between 00:00 and 23:59
        public static string FromTime(int hours, int minutes)
        {
            if (hours < 0 || hours >= 24)
                throw new ArgumentOutOfRangeException(nameof(hours));
            if (minutes < 0 || minutes >= 60)
                throw new ArgumentOutOfRangeException(nameof(minutes));

            return $"{hours:D2}:{minutes:D2}";
        }

        public static int ToMinutes(string actime)
        {
            var (hours, minutes) = ToTime(actime);
            return hours * 60 + minutes;
        }

        public static (int Hours, int Minutes) ToTime(string actime)
        {
            if (actime == null)
                throw new ArgumentNullException(nameof(actime));

            var times = actime.Split(':');
            if (times.Length != 2)
                throw new ArgumentException("Wrong input format.", nameof(actime));

            if (!int.TryParse(times[0], out var hours) || !int.TryParse(times[1], out var minutes))
            {
                throw new ArgumentException("Value cannot be parsed.", nameof(actime));
            }

            return (hours, minutes);
        }
    }
}
