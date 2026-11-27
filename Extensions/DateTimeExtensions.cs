using System;

namespace EcommerceStore.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo EasternZone = ResolveEasternTimeZone();

        private static TimeZoneInfo ResolveEasternTimeZone()
        {
            string[] timeZoneIds = { "Eastern Standard Time", "America/New_York", "US/Eastern" };
            foreach (var id in timeZoneIds)
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }

            // Fallback to UTC-5 fixed offset if Eastern timezone is not found
            return TimeZoneInfo.CreateCustomTimeZone("EST_Fallback", TimeSpan.FromHours(-5), "Eastern Standard Time", "EST");
        }

        /// <summary>
        /// Converts a UTC or unspecified DateTime to Eastern Standard / Daylight Time.
        /// </summary>
        public static DateTime ToEst(this DateTime dateTime)
        {
            DateTime utcDateTime = dateTime.Kind switch
            {
                DateTimeKind.Utc => dateTime,
                DateTimeKind.Local => dateTime.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            };

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, EasternZone);
        }

        /// <summary>
        /// Converts a nullable DateTime to Eastern Time if present.
        /// </summary>
        public static DateTime? ToEst(this DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToEst() : null;
        }

        /// <summary>
        /// Formats a DateTime in Eastern Time, optionally appending the EST/EDT zone tag.
        /// </summary>
        public static string ToEstFormat(this DateTime dateTime, string format = "MMM dd, yyyy h:mm tt", bool includeZone = true)
        {
            var estTime = dateTime.ToEst();
            var formatted = estTime.ToString(format);
            if (!includeZone) return formatted;

            string zoneTag = EasternZone.IsDaylightSavingTime(estTime) ? "EDT" : "EST";
            return $"{formatted} {zoneTag}";
        }

        /// <summary>
        /// Formats a nullable DateTime in Eastern Time.
        /// </summary>
        public static string ToEstFormat(this DateTime? dateTime, string format = "MMM dd, yyyy h:mm tt", bool includeZone = true)
        {
            return dateTime.HasValue ? dateTime.Value.ToEstFormat(format, includeZone) : string.Empty;
        }
    }
}
