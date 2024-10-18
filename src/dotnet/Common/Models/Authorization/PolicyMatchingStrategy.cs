using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Represents a policy matching strategy.
    /// </summary>
    public class PolicyMatchingStrategy
    {
        /// <summary>
        /// Gets or sets the list of user identity properties that are used to enforce the policy.
        /// </summary>
        /// <remarks>
        /// The specified user identity properties values must be matched by values of properties in the resources.
        /// It is up to the policy enforcement point to determine how to match the values
        /// (including which resource properties to use when matching).
        /// </remarks>
        [JsonPropertyName("user_identity_properties")]
        public List<string> UserIdentityProperties { get; set; } = [];
    }
}
