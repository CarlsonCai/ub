using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UmbracoSite.Json;

public sealed class LenientDateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private static readonly string[] Formats = ["yyyy-MM-dd", "yyyy/MM/dd"];

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var s = reader.GetString();
            if (string.IsNullOrWhiteSpace(s))
            {
                return default;
            }

            if (DateOnly.TryParseExact(s, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            if (DateOnly.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return date;
            }
        }

        throw new JsonException("Invalid DateOnly format. Use YYYY-MM-DD.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }
}

