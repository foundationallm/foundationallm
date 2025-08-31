using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Parameters for querying role assignments.
    /// </summary>
    public class SecurityPrincipalQueryParameters
    {
        /// <summary>
        /// Gets of sets the list of security principal identifiers to query.
        /// </summary>
        [JsonPropertyName("ids")]
        public required string[] Ids { get; set; }
    }
}
