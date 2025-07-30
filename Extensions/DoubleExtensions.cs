using System;

namespace ZapReport.Extensions
{
    public static class DoubleExtensions
    {
        public static double ToDegrees(this double value)
        {
            return 360.0 / Math.PI * value / 2;
        }

        public static string ToMinSec(this double value)
        {
            var min = Math.Floor(value / 60.0);
            var sec = value - min * 60.0;

            return $"{min:00}:{sec:00.000}";
        }

        public static string ToHourMinSec(this double value)
        {
            var hour = Math.Floor(value / 3600);
            var min = Math.Floor((value - hour * 3600) / 60.0);
            var sec = value - hour * 3600 - min * 60.0;

            return $"{hour:00}:{min:00}:{sec:00}";
        }
    }
}
