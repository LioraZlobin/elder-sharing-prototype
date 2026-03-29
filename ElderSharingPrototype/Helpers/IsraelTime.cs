using System;

namespace ElderSharingPrototype.Helpers
{
    public static class IsraelTime
    {
        private static readonly TimeZoneInfo IsraelZone =
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Jerusalem");

        public static DateTime Now()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, IsraelZone);
        }

        public static string NowHHmm()
        {
            return Now().ToString("HH:mm");
        }

        public static string NowMinuteKey()
        {
            return Now().ToString("yyyy-MM-dd HH:mm");
        }
    }
}