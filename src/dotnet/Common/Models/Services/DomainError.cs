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
        /// Gets a stable, machine-readable error type (like "file.rejected").
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; init; } = default!;

        /// <summary>
        /// Gets a short, human-readable title of the error.
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; init; } = default!;

        /// <summary>
        /// Gets the status code associated with the response or operation.
        /// </summary>
        [JsonPropertyName("status")]
        public int? Status { get; init; }

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
        /// Converts the current instance to a new <see cref="ProblemDetails"/> object with equivalent values.
        /// </summary>
        /// <returns>A <see cref="ProblemDetails"/> instance containing the type, title, detail, instance, and extensions from
        /// the current object.</returns>
        public ProblemDetails ToProblemDetails() =>
            new()
            {
                Type = this.Type,
                Title = this.Title,
                Status = this.Status,
                Detail = this.Detail,
                Instance = this.Instance,
                Extensions = new Dictionary<string, object?>(this.Extensions)
            };

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
                Type = problem?.Type ?? "http-error",
                Title = problem?.Title ?? $"HTTP {response.StatusCode}",
                Status = problem?.Status ?? (int)response.StatusCode,
                Detail = problem?.Detail ?? response.ReasonPhrase,
                Extensions = (problem?.Extensions ?? new Dictionary<string, object?>()).AsReadOnly()
            };
        }

        /// <summary>
        /// Creates a new instance of the DomainError class using the specified error message as the detail.
        /// </summary>
        /// <param name="errorMessage">The error message that describes the domain error. Cannot be null.</param>
        /// <param name="status">The status code associated with the error message.</param>
        /// <param name="instance">An optional instance detail about where the error occurred.</param>
        /// <returns>A DomainError instance with the specified error message set as the detail.</returns>
        public static DomainError FromErrorMessage(
            string errorMessage,
            int status = 500,
            string? instance = null) =>
            new()
            {
                Type = "generic-error",
                Title = "Generic Error",
                Status = status,
                Instance = instance,
                Detail = errorMessage
            };

        /// <summary>
        /// Creates a new DomainError instance that represents the specified exception.
        /// </summary>
        /// <param name="ex">The exception to convert into a DomainError. Cannot be null.</param>
        /// <param name="status">The status code associated with the exception.</param>
        /// <param name="instance">An optional instance detail about where the exception occurred.</param>
        /// <returns>A DomainError instance containing information from the specified exception.</returns>
        public static DomainError FromException(
            Exception ex,
            int status = 500,
            string? instance = null) =>
            new()
            {
                Type = "exception",
                Title = ex.GetType().Name,
                Status = status,
                Instance = instance,
                Detail = ex.Message
            };
    }
}
