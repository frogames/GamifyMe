using System;

namespace GamifyMe.UI.Shared.Helpers
{
    public static class TimeFormatter
    {
        public static string GetTimeRemaining(TimeSpan remaining)
        {
            if (remaining.TotalHours > 24)
            {
                return $"{remaining.Days}j {remaining.Hours}h restants";
            }
            else if (remaining.TotalHours > 0)
            {
                // For less than 24h, we can show hours and minutes
                // Or if it's very short (e.g. < 1h), maybe minutes and seconds?
                // The user asked to "add the number of days if > 24h".
                // For < 24h, the previous logic in Groups was "Xh Ym restants".
                // But for short countdowns (like cooldowns), seconds might be useful.
                // Let's stick to the requested format for > 24h, and keep existing behavior or reasonable default for < 24h.
                
                if (remaining.TotalHours < 1)
                {
                     return $"{remaining.Minutes}m {remaining.Seconds}s restants";
                }
                return $"{remaining.Hours}h {remaining.Minutes}m restants";
            }
            else
            {
                return "Terminé";
            }
        }

        // Overload accepting DateTime end
        public static string GetTimeRemaining(DateTime endDate)
        {
            var remaining = endDate - DateTime.UtcNow;
            if (remaining.TotalSeconds <= 0) return "Terminé";
            return GetTimeRemaining(remaining);
        }
    }
}
