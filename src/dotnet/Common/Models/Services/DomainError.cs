using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Services
{
    /// <summary>
    /// Represents a domain-specific error (RFC 7807 style).
    /// </summary>
    public class DomainError
    {
        /// <summary>
        /// Gets or sets a stable, machine-readable error type (like "file.rejected").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; init; } = default!;

        /// <summary>
        /// Gets or sets a short, human-readable title of the error.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; init; } = default!;

        /// <summary>
        /// Gets or sets a specific, contextual explanation.
        /// </summary>
        [JsonPropertyName("detail")]
        public string? Detail { get; init; }

        /// <summary>
        /// Gets or sets an optional detail about where did the error happen (operation name, aggregate id, etc.).
        /// </summary>
        [JsonPropertyName("instance")]
        public string? Instance { get; init; }

        /// <summary>
        /// Gets or sets extra data – like ProblemDetails.Extensions.
        /// </summary>
        [JsonPropertyName("extensions")]
        public IReadOnlyDictionary<string, object?> Extensions { get; init; }
            = new Dictionary<string, object?>();

        /// <summary>
        /// Creates a new instance of the DomainError class by parsing the specified HTTP response for problem details.
        /// </summary>
        /// <remarks>If the response content cannot be deserialized as ProblemDetails, default error
        /// information is used. This method does not throw on deserialization errors.</remarks>
        /// <param name="response">The HTTP response message to extract problem details from. Must not be null.</param>
        /// <returns>A DomainError instance populated with information from the response's problem details, or with default
        /// values if the response does not contain valid problem details.</returns>
        public static async Task<DomainError> FromHttpResponse(HttpResponseMessage response)
        {
            ProblemDetails? problem = null;
            try
            {
                problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            }
            catch
            {
                // Ignore deserialization errors
            }

            return new DomainError
            {
                Type = problem?.Type ?? "unknown-error",
                Title = problem?.Title ?? $"HTTP {response.StatusCode}",
                Detail = problem?.Detail ?? response.ReasonPhrase,
                Extensions = (problem?.Extensions ?? new Dictionary<string, object?>()).AsReadOnly()
            };
        }
    }
}
