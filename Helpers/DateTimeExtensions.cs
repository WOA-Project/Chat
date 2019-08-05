using System;

namespace Chat.Helpers
{
    // From http://www.blackbeltcoder.com/Articles/time/a-friendly-datetime-formatter
    public static class DateTimeExensions
    {
        /// <summary>
        /// Returns a string representation of this date/time. If the
        /// value is close to now, a relative description is returned.
        /// </summary>
        public static string ToRelativeString(this DateTimeOffset dt)
        {
            TimeSpan span = (DateTime.Now - dt);

            // Normalize time span
            bool future = false;
            if (span.TotalSeconds < 0)
            {
                // In the future
                span = -span;
                future = true;
            }

            // Test for Now
            double totalSeconds = span.TotalSeconds;
            if (totalSeconds < 0.9)
            {
                return "Now";
            }

            // Date/time near current date/time
            string format = (future) ? "In {0} {1}" : "{0} {1} ago";
            if (totalSeconds < 55)
            {
                // Seconds
                int seconds = Math.Max(1, span.Seconds);
                return String.Format(format, seconds,
                    (seconds == 1) ? "second" : "seconds");
            }

            if (totalSeconds < (55 * 60))
            {
                // Minutes
                int minutes = Math.Max(1, span.Minutes);
                return String.Format(format, minutes,
                    (minutes == 1) ? "minute" : "minutes");
            }
            if (totalSeconds < (24 * 60 * 60))
            {
                // Hours
                int hours = Math.Max(1, span.Hours);
                return String.Format(format, hours,
                    (hours == 1) ? "hour" : "hours");
            }

            // Format both date and time
            if (totalSeconds < (48 * 60 * 60))
            {
                // 1 Day
                format = (future) ? "Tomorrow" : "Yesterday";
            }
            else if (totalSeconds < (3 * 24 * 60 * 60))
            {
                // 2 Days
                format = String.Format(format, 2, "days");
            }
            else
            {
                // Absolute date
                if (dt.Year == DateTime.Now.Year)
                    format = dt.ToString(@"MMM d");
                else
                    format = dt.ToString(@"MMM d, yyyy");
            }

            // Add time
            return String.Format("{0} at {1:h:mmt}", format, dt);
        }
    }
}
