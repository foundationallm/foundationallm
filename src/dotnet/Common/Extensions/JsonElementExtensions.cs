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
                JsonValueKind.Number => jsonElement.TryGetInt32(out var intValue)
                    ? intValue
                    : jsonElement.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => throw new NotSupportedException($"Unsupported JSON value kind: {jsonElement.ValueKind}")
            };
    }
}
