using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authentication
{
    /// <summary>
    /// The result of an object query.
    /// </summary>
    public class ObjectQueryResult
    {
        /// <summary>
        /// The unique identifier of the object.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// User account email address.
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>
        /// The display name of the object.
        /// </summary>
        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the name of the on-premises account associated with the security principal.
        /// </summary>
        [JsonPropertyName("onpremises_account_name")]
        public string? OnPremisesAccountName { get; set; }

        /// <summary>
        /// The type of the object.
        /// </summary>
        [JsonPropertyName("object_type")]
        public string? ObjectType { get; set; }
    }
}
