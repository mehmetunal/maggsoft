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
}
