using FoundationaLLM.Common.Models.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Defines a security policy used in FoundationaLLM PBAC
    /// </summary>
    public class PolicyDefinition : ResourceBase
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override bool Deleted { get; set; }

        /// <summary>
        /// The list of scopes at which the role can be assigned.
        /// </summary>
        [JsonPropertyName("assignable_scopes")]
        [JsonPropertyOrder(1)]
        public List<string> AssignableScopes { get; set; } = [];

        /// <summary>
        /// Gets or sets the <see cref="PolicyMatchingStrategy"/> value defining how the policy should be matched.
        /// </summary>
        [JsonPropertyName("matching_strategy")]
        [JsonPropertyOrder(2)]
        public required PolicyMatchingStrategy MatchingStrategy { get; set; }
    }
}
