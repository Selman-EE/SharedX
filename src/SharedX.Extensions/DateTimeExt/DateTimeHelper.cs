namespace SharedX.Extensions.DateTimeExt;

/// <summary>
///     Provides extension methods for DateTime operations with UTC-first design.
///     All methods assume UTC unless explicitly stated otherwise.
/// </summary>
public static class DateTimeHelper
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    #region Unix Timestamp Conversions

    /// <summary>
    ///     Converts a UTC DateTime to Unix timestamp (seconds since 1970-01-01 00:00:00 UTC).
    /// </summary>
    /// <param name="dateTime">The UTC DateTime to convert.</param>
    /// <returns>Unix timestamp in seconds.</returns>
    /// <exception cref="ArgumentException">Thrown when dateTime is not UTC.</exception>
    /// <remarks>
    ///     For DateTimeKind.Unspecified, assumes UTC. For safety, always use DateTime.UtcNow or ensure DateTimeKind.Utc.
    /// </remarks>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Local)
            throw new ArgumentException("Cannot convert local time to Unix timestamp. Convert to UTC first.",
                nameof(dateTime));

        var utcDateTime = dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        return (long)(utcDateTime - UnixEpoch).TotalSeconds;
    }

    /// <summary>
    ///     Converts a UTC DateTime to Unix timestamp in milliseconds (since 1970-01-01 00:00:00 UTC).
    /// </summary>
    /// <param name="dateTime">The UTC DateTime to convert.</param>
    /// <returns>Unix timestamp in milliseconds.</returns>
    /// <exception cref="ArgumentException">Thrown when dateTime is not UTC.</exception>
    public static long ToUnixTimestampMilliseconds(this DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Local)
            throw new ArgumentException("Cannot convert local time to Unix timestamp. Convert to UTC first.",
                nameof(dateTime));

        var utcDateTime = dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        return (long)(utcDateTime - UnixEpoch).TotalMilliseconds;
    }

    /// <summary>
    ///     Converts a Unix timestamp (seconds since 1970-01-01 00:00:00 UTC) to a UTC DateTime.
    /// </summary>
    /// <param name="timestamp">Unix timestamp in seconds.</param>
    /// <returns>A UTC DateTime.</returns>
    public static DateTime FromUnixTimestamp(this long timestamp)
    {
        return UnixEpoch.AddSeconds(timestamp);
    }

    /// <summary>
    ///     Converts a Unix timestamp in milliseconds to a UTC DateTime.
    /// </summary>
    /// <param name="timestampMilliseconds">Unix timestamp in milliseconds.</param>
    /// <returns>A UTC DateTime.</returns>
    public static DateTime FromUnixTimestampMilliseconds(this long timestampMilliseconds)
    {
        return UnixEpoch.AddMilliseconds(timestampMilliseconds);
    }

    #endregion

    #region Day Boundaries (UTC)

    /// <summary>
    ///     Returns the start of the day (00:00:00) in UTC.
    /// </summary>
    /// <param name="dateTime">The UTC DateTime.</param>
    /// <returns>A UTC DateTime representing the start of the day.</returns>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    ///     Returns the end of the day (23:59:59.999) in UTC.
    /// </summary>
    /// <param name="dateTime">The UTC DateTime.</param>
    /// <returns>A UTC DateTime representing the end of the day.</returns>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999, DateTimeKind.Utc)
            .AddTicks(9999); // 23:59:59.9999999
    }

    /// <summary>
    ///     Returns the start of the week (Monday 00:00:00) in UTC.
    /// </summary>
    /// <param name="dateTime">The UTC DateTime.</param>
    /// <returns>A UTC DateTime representing the start of the week.</returns>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        var diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
        return dateTime.AddDays(-diff).StartOfDay();
    }

    /// <summary>
    ///     Returns the start of the month (1st day 00:00:00) in UTC.
    /// </summary>
    /// <param name="dateTime">The UTC DateTime.</param>
    /// <returns>A UTC DateTime representing the start of the month.</returns>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    ///     Returns the start of the year (January 1st 00:00:00) in UTC.
    /// </summary>
    /// <param name="dateTime">The UTC DateTime.</param>
    /// <returns>A UTC DateTime representing the start of the year.</returns>
    public static DateTime StartOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    #endregion

    #region Business Days

    /// <summary>
    ///     Determines whether the specified date is a weekend (Saturday or Sunday).
    ///     Culture-independent check.
    /// </summary>
    /// <param name="dateTime">The date to check.</param>
    /// <returns>true if the date is a weekend; otherwise, false.</returns>
    public static bool IsWeekend(this DateTime dateTime)
    {
        return dateTime.DayOfWeek == DayOfWeek.Saturday || dateTime.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    ///     Determines whether the specified date is a weekday (Monday through Friday).
    ///     Culture-independent check.
    /// </summary>
    /// <param name="dateTime">The date to check.</param>
    /// <returns>true if the date is a weekday; otherwise, false.</returns>
    public static bool IsWeekday(this DateTime dateTime)
    {
        return !dateTime.IsWeekend();
    }

    /// <summary>
    ///     Adds the specified number of business days to the date, skipping weekends.
    /// </summary>
    /// <param name="dateTime">The starting date.</param>
    /// <param name="businessDays">The number of business days to add (can be negative).</param>
    /// <returns>A DateTime with the business days added.</returns>
    /// <remarks>
    ///     Does not account for holidays. Use <see cref="AddBusinessDays(System.DateTime, int, System.DateTime[])" />
    ///     to exclude specific holiday dates.
    /// </remarks>
    public static DateTime AddBusinessDays(this DateTime dateTime, int businessDays)
    {
        return dateTime.AddBusinessDays(businessDays, Array.Empty<DateTime>());
    }

    /// <summary>
    ///     Adds the specified number of business days to the date, skipping weekends and specified holidays.
    /// </summary>
    /// <param name="dateTime">The starting date.</param>
    /// <param name="businessDays">The number of business days to add (can be negative).</param>
    /// <param name="holidays">Array of holiday dates to skip (should be in UTC).</param>
    /// <returns>A DateTime with the business days added.</returns>
    public static DateTime AddBusinessDays(this DateTime dateTime, int businessDays,
        DateTime[] holidays)
    {
        var sign = businessDays < 0 ? -1 : 1;
        var unsignedDays = Math.Abs(businessDays);
        var result = dateTime;

        for (var i = 0; i < unsignedDays; i++)
            do
            {
                result = result.AddDays(sign);
            } while (result.IsWeekend() || IsHoliday(result, holidays));

        return result;
    }

    private static bool IsHoliday(DateTime date, DateTime[] holidays)
    {
        if (holidays == null || holidays.Length == 0)
            return false;

        var dateOnly = date.Date;
        foreach (var holiday in holidays)
            if (holiday.Date == dateOnly)
                return true;

        return false;
    }

    #endregion

    #region Timezone Conversions

    /// <summary>
    ///     Converts a UTC DateTime to the specified timezone and returns a DateTimeOffset.
    /// </summary>
    /// <param name="utcDateTime">The UTC DateTime to convert.</param>
    /// <param name="timeZone">The target timezone.</param>
    /// <returns>A DateTimeOffset in the specified timezone.</returns>
    /// <exception cref="ArgumentNullException">Thrown when timeZone is null.</exception>
    /// <exception cref="ArgumentException">Thrown when utcDateTime is not UTC.</exception>
    /// <remarks>
    ///     Example: utcDateTime.ToTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw"))
    /// </remarks>
    public static DateTimeOffset ToTimeZone(this DateTime utcDateTime, TimeZoneInfo timeZone)
    {
        if (timeZone == null)
            throw new ArgumentNullException(nameof(timeZone));

        if (utcDateTime.Kind == DateTimeKind.Local)
            throw new ArgumentException("Cannot convert local time. Convert to UTC first.", nameof(utcDateTime));

        var utc = utcDateTime.Kind == DateTimeKind.Utc
            ? utcDateTime
            : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTime(new DateTimeOffset(utc), timeZone);
    }

    /// <summary>
    ///     Converts a DateTimeOffset to the specified timezone.
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to convert.</param>
    /// <param name="timeZone">The target timezone.</param>
    /// <returns>A DateTimeOffset in the specified timezone.</returns>
    /// <exception cref="ArgumentNullException">Thrown when timeZone is null.</exception>
    public static DateTimeOffset ToTimeZone(this DateTimeOffset dateTimeOffset, TimeZoneInfo timeZone)
    {
        if (timeZone == null)
            throw new ArgumentNullException(nameof(timeZone));

        return TimeZoneInfo.ConvertTime(dateTimeOffset, timeZone);
    }

    #endregion

    #region Formatting

    /// <summary>
    ///     Converts the DateTime to ISO 8601 format (yyyy-MM-ddTHH:mm:ss.fffZ).
    /// </summary>
    /// <param name="dateTime">The UTC DateTime to format.</param>
    /// <returns>ISO 8601 formatted string.</returns>
    /// <remarks>
    ///     The 'Z' suffix indicates UTC time. If dateTime is not UTC, it will be treated as UTC for formatting.
    /// </remarks>
    public static string ToIso8601(this DateTime dateTime)
    {
        var utc = dateTime.Kind == DateTimeKind.Utc
            ? dateTime
            : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);

        return utc.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }

    /// <summary>
    ///     Converts the DateTimeOffset to ISO 8601 format with timezone offset.
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to format.</param>
    /// <returns>ISO 8601 formatted string with timezone offset.</returns>
    public static string ToIso8601(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz");
    }

    #endregion

    #region Age Calculations

    /// <summary>
    ///     Calculates the age in years from the birth date to the current UTC date.
    /// </summary>
    /// <param name="birthDate">The birth date.</param>
    /// <returns>Age in years.</returns>
    public static int GetAge(this DateTime birthDate)
    {
        return birthDate.GetAge(DateTime.UtcNow);
    }

    /// <summary>
    ///     Calculates the age in years from the birth date to a specific date.
    /// </summary>
    /// <param name="birthDate">The birth date.</param>
    /// <param name="asOfDate">The date to calculate age as of.</param>
    /// <returns>Age in years.</returns>
    public static int GetAge(this DateTime birthDate, DateTime asOfDate)
    {
        var age = asOfDate.Year - birthDate.Year;
        if (asOfDate.Month < birthDate.Month ||
            (asOfDate.Month == birthDate.Month && asOfDate.Day < birthDate.Day))
            age--;

        return age;
    }

    #endregion
}