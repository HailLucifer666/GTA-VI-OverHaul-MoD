using System;

namespace ClassicGTA.Adapter.GTAV
{
    /// <summary>
    /// Single absolute UTC calendar-day clock for passive business income (I-01).
    /// Zero dependency on GTA.Game.GameTime or session duration so restarts never go backward.
    /// </summary>
    public static class PassiveIncomeClock
    {
        public static readonly DateTime EpochUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int GetUtcDay(DateTime utcNow)
        {
            DateTime normalized = utcNow.Kind == DateTimeKind.Utc ? utcNow : utcNow.ToUniversalTime();
            double days = Math.Floor((normalized - EpochUtc).TotalDays);
            if (days < 0) return 0;
            if (days > int.MaxValue) return int.MaxValue;
            return (int)days;
        }
    }
}
