using FluentAssertions;
using SharedX.Extensions.DateTime;
using System;
using Xunit;

namespace SharedX.Tests.ExtensionTests
{
    public class DateTimeExtensionsTests
    {
        #region Unix Timestamp Tests

        [Fact]
        public void ToUnixTimestamp_UnixEpoch_ReturnsZero()
        {
            // Arrange
            var epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = epoch.ToUnixTimestamp();

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public void ToUnixTimestamp_KnownDate_ReturnsCorrectTimestamp()
        {
            // Arrange - 2025-01-01 00:00:00 UTC
            var date = new System.DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var result = date.ToUnixTimestamp();

            // Assert
            result.Should().Be(1735689600); // Known Unix timestamp
        }

        [Fact]
        public void FromUnixTimestamp_Zero_ReturnsUnixEpoch()
        {
            // Act
            var result = 0L.FromUnixTimestamp();

            // Assert
            result.Should().Be(new System.DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public void FromUnixTimestamp_KnownTimestamp_ReturnsCorrectDate()
        {
            // Arrange
            var timestamp = 1735689600L; // 2025-01-01 00:00:00 UTC

            // Act
            var result = timestamp.FromUnixTimestamp();

            // Assert
            result.Year.Should().Be(2025);
            result.Month.Should().Be(1);
            result.Day.Should().Be(1);
            result.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void UnixTimestamp_RoundTrip_PreservesValue()
        {
            // Arrange
            var original = System.DateTime.UtcNow;

            // Act
            var timestamp = original.ToUnixTimestamp();
            var result = timestamp.FromUnixTimestamp();

            // Assert - Allow 1 second difference due to rounding
            (result - original).TotalSeconds.Should().BeLessThan(1);
        }

        [Fact]
        public void ToUnixTimestamp_LocalTime_ThrowsException()
        {
            // Arrange
            var localTime = System.DateTime.Now;

            // Act & Assert
            var act = () => localTime.ToUnixTimestamp();
            act.Should().Throw<ArgumentException>()
                .WithMessage("*local time*");
        }

        [Fact]
        public void ToUnixTimestampMilliseconds_PreservesMilliseconds()
        {
            // Arrange
            var date = new System.DateTime(2025, 1, 1, 0, 0, 0, 123, DateTimeKind.Utc);

            // Act
            var timestamp = date.ToUnixTimestampMilliseconds();
            var result = timestamp.FromUnixTimestampMilliseconds();

            // Assert
            result.Millisecond.Should().Be(123);
        }

        #endregion

        #region Day Boundaries Tests

        [Fact]
        public void StartOfDay_ReturnsCorrectTime()
        {
            // Arrange
            var date = new System.DateTime(2025, 10, 29, 15, 30, 45, DateTimeKind.Utc);

            // Act
            var result = date.StartOfDay();

            // Assert
            result.Year.Should().Be(2025);
            result.Month.Should().Be(10);
            result.Day.Should().Be(29);
            result.Hour.Should().Be(0);
            result.Minute.Should().Be(0);
            result.Second.Should().Be(0);
            result.Millisecond.Should().Be(0);
            result.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void EndOfDay_ReturnsCorrectTime()
        {
            // Arrange
            var date = new System.DateTime(2025, 10, 29, 10, 30, 45, DateTimeKind.Utc);

            // Act
            var result = date.EndOfDay();

            // Assert
            result.Year.Should().Be(2025);
            result.Month.Should().Be(10);
            result.Day.Should().Be(29);
            result.Hour.Should().Be(23);
            result.Minute.Should().Be(59);
            result.Second.Should().Be(59);
            result.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void StartOfWeek_Monday_ReturnsSameDay()
        {
            // Arrange - Monday, October 27, 2025
            var monday = new System.DateTime(2025, 10, 27, 10, 0, 0, DateTimeKind.Utc);

            // Act
            var result = monday.StartOfWeek();

            // Assert
            result.Date.Should().Be(monday.Date);
            result.DayOfWeek.Should().Be(DayOfWeek.Monday);
        }

        [Fact]
        public void StartOfWeek_Sunday_ReturnsPreviousMonday()
        {
            // Arrange - Sunday, November 2, 2025
            var sunday = new System.DateTime(2025, 11, 2, 10, 0, 0, DateTimeKind.Utc);

            // Act
            var result = sunday.StartOfWeek();

            // Assert
            result.DayOfWeek.Should().Be(DayOfWeek.Monday);
            result.Date.Should().Be(new System.DateTime(2025, 10, 27));
        }

        [Fact]
        public void StartOfMonth_ReturnsFirstDayOfMonth()
        {
            // Arrange
            var date = new System.DateTime(2025, 10, 29, 15, 30, 45, DateTimeKind.Utc);

            // Act
            var result = date.StartOfMonth();

            // Assert
            result.Day.Should().Be(1);
            result.Month.Should().Be(10);
            result.Year.Should().Be(2025);
            result.Hour.Should().Be(0);
        }

        [Fact]
        public void StartOfYear_ReturnsJanuaryFirst()
        {
            // Arrange
            var date = new System.DateTime(2025, 10, 29, 15, 30, 45, DateTimeKind.Utc);

            // Act
            var result = date.StartOfYear();

            // Assert
            result.Month.Should().Be(1);
            result.Day.Should().Be(1);
            result.Year.Should().Be(2025);
            result.Hour.Should().Be(0);
        }

        #endregion

        #region Business Days Tests

        [Theory]
        [InlineData(2025, 11, 1, true)]  // Saturday
        [InlineData(2025, 11, 2, true)]  // Sunday
        [InlineData(2025, 11, 3, false)] // Monday
        [InlineData(2025, 11, 7, false)] // Friday
        public void IsWeekend_VariousDays_ReturnsExpectedResult(int year, int month, int day, bool expected)
        {
            // Arrange
            var date = new System.DateTime(year, month, day, 12, 0, 0, DateTimeKind.Utc);

            // Act
            var result = date.IsWeekend();

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void AddBusinessDays_PositiveDays_SkipsWeekends()
        {
            // Arrange - Friday, October 31, 2025
            var friday = new System.DateTime(2025, 10, 31, 12, 0, 0, DateTimeKind.Utc);

            // Act - Add 3 business days
            var result = friday.AddBusinessDays(3);

            // Assert - Should be Wednesday, November 5 (skips Sat, Sun, Mon, Tue)
            result.DayOfWeek.Should().Be(DayOfWeek.Wednesday);
            result.Date.Should().Be(new System.DateTime(2025, 11, 5));
        }

        [Fact]
        public void AddBusinessDays_NegativeDays_SkipsWeekends()
        {
            // Arrange - Monday, November 3, 2025
            var monday = new System.DateTime(2025, 11, 3, 12, 0, 0, DateTimeKind.Utc);

            // Act - Subtract 3 business days
            var result = monday.AddBusinessDays(-3);

            // Assert - Should be Wednesday, October 29
            result.DayOfWeek.Should().Be(DayOfWeek.Wednesday);
            result.Date.Should().Be(new System.DateTime(2025, 10, 29));
        }

        [Fact]
        public void AddBusinessDays_WithHolidays_SkipsHolidays()
        {
            // Arrange - Thursday, October 30, 2025
            var thursday = new System.DateTime(2025, 10, 30, 12, 0, 0, DateTimeKind.Utc);
            var holidays = new[]
            {
                new System.DateTime(2025, 10, 31, 0, 0, 0, DateTimeKind.Utc) // Friday holiday
            };

            // Act - Add 1 business day
            var result = thursday.AddBusinessDays(1, holidays);

            // Assert - Should skip Friday (holiday) and weekend, land on Monday
            result.DayOfWeek.Should().Be(DayOfWeek.Monday);
            result.Date.Should().Be(new System.DateTime(2025, 11, 3));
        }

        #endregion

        #region Timezone Conversion Tests

        [Fact]
        public void ToTimeZone_UtcToEuropeanTime_ConvertsCorrectly()
        {
            // Arrange
            var utcTime = new System.DateTime(2025, 10, 29, 12, 0, 0, DateTimeKind.Utc);
            var warsawTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

            // Act
            var result = utcTime.ToTimeZone(warsawTz);

            // Assert
            result.Offset.Should().Be(TimeSpan.FromHours(1)); // CET (UTC+1) in October
            result.Hour.Should().Be(13); // 12 UTC = 13 CET
        }

        [Fact]
        public void ToTimeZone_NullTimeZone_ThrowsException()
        {
            // Arrange
            var utcTime = System.DateTime.UtcNow;

            // Act & Assert
            var act = () => utcTime.ToTimeZone(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ToTimeZone_LocalTime_ThrowsException()
        {
            // Arrange
            var localTime = System.DateTime.Now;
            var warsawTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

            // Act & Assert
            var act = () => localTime.ToTimeZone(warsawTz);
            act.Should().Throw<ArgumentException>()
                .WithMessage("*local time*");
        }

        #endregion

        #region Formatting Tests

        [Fact]
        public void ToIso8601_UtcDateTime_FormatsCorrectly()
        {
            // Arrange
            var date = new System.DateTime(2025, 10, 29, 14, 30, 45, 123, DateTimeKind.Utc);

            // Act
            var result = date.ToIso8601();

            // Assert
            result.Should().Be("2025-10-29T14:30:45.123Z");
        }

        [Fact]
        public void ToIso8601_DateTimeOffset_FormatsWithOffset()
        {
            // Arrange
            var offset = new DateTimeOffset(2025, 10, 29, 14, 30, 45, 123, TimeSpan.FromHours(2));

            // Act
            var result = offset.ToIso8601();

            // Assert
            result.Should().Be("2025-10-29T14:30:45.123+02:00");
        }

        #endregion

        #region Age Calculation Tests

        [Fact]
        public void GetAge_ExactYears_ReturnsCorrectAge()
        {
            // Arrange
            var birthDate = new System.DateTime(2000, 10, 29, 0, 0, 0, DateTimeKind.Utc);
            var asOfDate = new System.DateTime(2025, 10, 29, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var age = birthDate.GetAge(asOfDate);

            // Assert
            age.Should().Be(25);
        }

        [Fact]
        public void GetAge_BeforeBirthday_ReturnsOneYearLess()
        {
            // Arrange
            var birthDate = new System.DateTime(2000, 10, 29, 0, 0, 0, DateTimeKind.Utc);
            var asOfDate = new System.DateTime(2025, 10, 28, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var age = birthDate.GetAge(asOfDate);

            // Assert
            age.Should().Be(24);
        }

        [Fact]
        public void GetAge_AfterBirthday_ReturnsCorrectAge()
        {
            // Arrange
            var birthDate = new System.DateTime(2000, 10, 29, 0, 0, 0, DateTimeKind.Utc);
            var asOfDate = new System.DateTime(2025, 10, 30, 0, 0, 0, DateTimeKind.Utc);

            // Act
            var age = birthDate.GetAge(asOfDate);

            // Assert
            age.Should().Be(25);
        }

        #endregion
    }
}
