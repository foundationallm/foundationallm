using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// The result of a resource name check.
    /// </summary>
    /// <param name="Name">The name of the resource.</param>
    /// <param name="Type">The type of the resource.</param>
    /// <param name="Status">The <see cref="NameCheckResultType"/> indicating whether the name is allowed or not.</param>
    /// <param name="Exists">Indicates whether the resource exists or not. For logically deleted resources, the value will be <c>true</c>.</param>
    /// <param name="Deleted">Indicates whether the resource is logically deleted or not.</param>
    /// <param name="ErrorMessage">An optional error message if the name check failed.</param>
    public record ResourceNameCheckResult(
        [property: JsonPropertyName("name")]
        string Name,
        [property: JsonPropertyName("type")]
        string? Type,
        [property: JsonPropertyName("status")]
        [property: JsonConverter(typeof(JsonStringEnumConverter))]
        NameCheckResultType Status,
        [property: JsonPropertyName("exists")]
        bool Exists,
        [property: JsonPropertyName("deleted")]
        bool Deleted,
        string? ErrorMessage = null) : ResourceProviderActionResult(string.Empty, true, ErrorMessage);

    /// <summary>
    /// The result types of resource name checks.
    /// </summary>
    public enum NameCheckResultType
    {
        /// <summary>
        /// The name is valid and is allowed.
        /// </summary>
        Allowed,
        /// <summary>
        /// The name is invalid and cannot be used
        /// </summary>
        Denied
    }
}
