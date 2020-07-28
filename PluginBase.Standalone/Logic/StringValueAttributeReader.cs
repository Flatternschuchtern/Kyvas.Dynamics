using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PluginBase.Standalone.PluginBase.Attributes;

namespace PluginBase.Standalone.Logic
{
    public static class StringValueAttributeReader
    {
        public static bool TryReadEnumValue<TEnum>(string input, out TEnum value)
        where TEnum : struct
        {
            var values =
                ((TEnum[])Enum.GetValues(typeof(TEnum)))
                .SelectMany(
                    f =>
                    {
                        var field = typeof(TEnum).GetField(f.ToString());
                        var attributes = (StringValueAttribute[])field.GetCustomAttributes(typeof(StringValueAttribute), false);
                        var pairs = new List<KeyValuePair<string, TEnum>>
                        {
                            new KeyValuePair<string, TEnum>(Enum.GetName(typeof(TEnum), f), f)
                        };
                        pairs
                            .AddRange(
                                attributes
                                    .Select(a =>
                                        new KeyValuePair<string, TEnum>(a.Value, f)));
                        return pairs;
                    })
                .ToDictionary(p => p.Key, p => p.Value);
            return values.TryGetValue(input, out value);
        }

        public static string GetFirstValueOrName(PropertyInfo property)
        {
            var attributes = property.GetCustomAttributes(typeof(StringValueAttribute)).OfType<StringValueAttribute>();
            return attributes.FirstOrDefault()?.Value ?? property.Name;
        }

        public static string GetFirstValueOrName(FieldInfo property)
        {
            var attributes = property.GetCustomAttributes(typeof(StringValueAttribute)).OfType<StringValueAttribute>();
            return attributes.FirstOrDefault()?.Value ?? property.Name;
        }
    }
}
