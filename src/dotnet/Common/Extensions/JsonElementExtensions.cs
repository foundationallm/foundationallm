using System.Text.Json;

namespace FoundationaLLM.Common.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="JsonElement"/>.
    /// </summary>
    public static class JsonElementExtensions
    {
        /// <summary>
        /// Converts the JSON element to the underlying object type.
        /// </summary>
        /// <param name="jsonElement">The JSON element to convert.</param>
        /// <returns>The underlying object type.</returns>
        /// <exception cref="NotSupportedException"></exception>
        public static object? ToObject(this JsonElement jsonElement) =>
            jsonElement.ValueKind switch
            {
                JsonValueKind.String => jsonElement.GetString()!,
                JsonValueKind.Number => GetNumber(jsonElement),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                JsonValueKind.Array => GetArray(jsonElement),
                _ => throw new NotSupportedException($"Unsupported JSON value kind: {jsonElement.ValueKind}")
            };

        private static object GetNumber(JsonElement jsonElement)
        {
            if (jsonElement.TryGetInt32(out var intValue))
            {
                return intValue;
            }
            if (jsonElement.TryGetInt64(out var longValue))
            {
                return longValue;
            }
            if (jsonElement.TryGetDouble(out var doubleValue))
            {
                return doubleValue;
            }
            throw new NotSupportedException($"Unsupported JSON number kind: {jsonElement.ValueKind}");
        }

        private static List<object?> GetArray(JsonElement jsonElement) =>
            [.. jsonElement.EnumerateArray().Select(e => e.ToObject())];
    }
}
