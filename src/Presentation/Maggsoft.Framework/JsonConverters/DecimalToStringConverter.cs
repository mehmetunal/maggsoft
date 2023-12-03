using System;
using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Maggsoft.Framework.JsonConverters;

public class DecimalToStringConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture("tr-TR"); // Türkçe
            var style = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;

            ReadOnlySpan<byte> span = reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan;
            if (Utf8Parser.TryParse(span, out decimal number, out int bytesConsumed) && span.Length == bytesConsumed)
            {
                return number;
            }

            if (Decimal.TryParse(reader.GetString(), style, cultureInfo, out number))
            {
                return number;
            }
        }

        return reader.GetDecimal();
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
