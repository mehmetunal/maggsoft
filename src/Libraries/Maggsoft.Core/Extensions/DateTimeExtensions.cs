using System;

namespace Maggsoft.Core.Extensions;

public static class DateTimeExtensions
{
    public static DateTime Next(this DateTime from, DayOfWeek dayOfWeek, bool includeToday = false)
    {
        int start = (int)from.DayOfWeek;
        int target = (int)dayOfWeek;

        var condition = target <= start;
        if (includeToday == true)
            condition = target < start;

        if (condition)
            target += 7;

        return from.AddDays(target - start);
    }

    /// <summary>
    /// Unix Epoch value.
    /// </summary>
    public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Converts DateTime to UnixTimestamp.
    /// </summary>
    /// <param name="dateTime">.</param>
    /// <returns>UnixTimestamp</returns>
    public static int DateTimeToUnixTimestamp(this DateTime dateTime)
    {
        return (int)(dateTime - Epoch).TotalSeconds;
    }
}
