using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Basic properties for all resources.
    /// </summary>
    public class ResourceBase : ResourceName
    {
        /// <summary>
        /// The unique identifier of the resource.
        /// </summary>
        [JsonPropertyName("object_id")]
        [JsonPropertyOrder(-4)]
        public string? ObjectId { get; set; }

        /// <summary>
        /// Gets the resource path associated with the resource's object identifier.
        /// </summary>
        [JsonIgnore]
        public ResourcePath ResourcePath =>
            ResourcePath.GetResourcePath(ObjectId!);

        /// <summary>
        /// The display name of the resource.
        /// </summary>
        [JsonPropertyName("display_name")]
        [JsonPropertyOrder(-3)]
        public string? DisplayName { get; set; }

        /// <summary>
        /// The description of the resource.
        /// </summary>
        [JsonPropertyName("description")]
        [JsonPropertyOrder(-2)]
        public string? Description { get; set; }

        /// <summary>
        /// The cost center of the resource.
        /// </summary>
        [JsonPropertyName("cost_center")]
        [JsonPropertyOrder(-1)]
        public string? CostCenter { get; set; }

        /// <summary>
        /// Gets or sets the property bag for the resource.
        /// </summary>
        [JsonPropertyName("properties")]
        [JsonPropertyOrder(400)]
        public Dictionary<string, string>? Properties { get; set; }

        /// <summary>
        /// The time at which the resource was created.
        /// </summary>
        [JsonPropertyName("created_on")]
        [JsonPropertyOrder(500)]
        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// The time at which the resource was last updated.
        /// </summary>
        [JsonPropertyName("updated_on")]
        [JsonPropertyOrder(501)]
        public DateTimeOffset UpdatedOn { get; set; }

        /// <summary>
        /// The entity who created the resource.
        /// </summary>
        [JsonPropertyName("created_by")]
        [JsonPropertyOrder(502)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// The entity who last updated the resource.
        /// </summary>
        [JsonPropertyName("updated_by")]
        [JsonPropertyOrder(503)]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Indicates whether the resource has been logically deleted.
        /// </summary>
        [JsonPropertyName("deleted")]
        [JsonPropertyOrder(504)]
        public virtual bool Deleted { get; set; } = false;

        /// <summary>
        /// The date and time on which the resource expires and is no longer usable.
        /// </summary>
        [JsonPropertyName("expiration_date")]
        [JsonPropertyOrder(505)]
        public DateTimeOffset? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the list of authorizable actions that are
        /// automatically inherited by child resources.
        /// </summary>
        [JsonPropertyName("inheritable_authorizable_actions")]
        [JsonPropertyOrder(506)]
        public HashSet<string> InheritableAuthorizableActions { get; set; } = [];
    }
}
