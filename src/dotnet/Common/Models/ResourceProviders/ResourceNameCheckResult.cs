using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// The result of a resource name check.
    /// </summary>
    public class ResourceNameCheckResult : ResourceName
    {
        /// <summary>
        /// The <see cref="NameCheckResultType"/> indicating whether the name is allowed or not.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NameCheckResultType Status { get; set; }

        /// <summary>
        /// An optional message indicating why is the name not allowed.
        /// </summary>
        public string? Message { get; set; }
    }

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
