using System;
using System.ComponentModel;

namespace Maggsoft.Core.Extensions;

public static class EnumExtension
{
    public static string GetEnumDescription(this Enum enumValue)
    {
        var field = enumValue.GetType().GetField(enumValue.ToString());
        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
        {
            return attribute.Description;
        }
        throw new ArgumentException("Item not found.", nameof(enumValue));
    }

    /// <summary>
    /// string iphone = "iPhone 13 Pro Max";
    //  FlagshipSmartphone flagship = iphone.GetEnumValueByDescription<FlagshipSmartphone>();
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="description"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static T GetEnumValueByDescription<T>(this string description) where T : Enum
    {
        foreach (Enum enumItem in Enum.GetValues(typeof(T)))
        {
            if (enumItem.GetEnumDescription() == description)
            {
                return (T)enumItem;
            }
        }
        throw new ArgumentException("Not found.", nameof(description));
    }
}
