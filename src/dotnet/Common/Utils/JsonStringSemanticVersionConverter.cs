using NuGet.Versioning;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Utils;

/// <summary>
/// Provides a JSON converter for <see cref="SemanticVersion"/> objects.
/// </summary>
public class JsonStringSemanticVersionConverter : JsonConverter<SemanticVersion>
{
    /// <inheritdoc/>
    public override SemanticVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var versionString = reader.GetString();
        if (SemanticVersion.TryParse(versionString!, out var version))
        {
            return version;
        }
        throw new JsonException($"Invalid semantic version: {versionString}");
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, SemanticVersion value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString());
}
