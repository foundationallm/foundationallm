using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// The result of an action executed by a resource provider.
    /// </summary>
    /// <param name="IsSuccessResult">Indicates whether the action executed successfully or not.</param>
    public record ResourceProviderActionResult<T>(
        bool IsSuccessResult) : ResourceProviderActionResult(IsSuccessResult)
        where T : ResourceBase
    {
        /// <summary>
        /// Gets or sets the resource resulting from the action.
        /// </summary>
        /// <remarks>
        /// Each resource provider will decide whether to return the resource in the action result or not.
        /// </remarks>
        [JsonPropertyName("resource")]
        public T? Resource { get; set; }
    }
}
