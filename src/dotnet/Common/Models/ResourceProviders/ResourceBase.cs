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
        /// The time at which the security role definition was created.
        /// </summary>
        [JsonPropertyName("created_on")]
        [JsonPropertyOrder(80)]
        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// The time at which the security role definition was last updated.
        /// </summary>
        [JsonPropertyName("updated_on")]
        [JsonPropertyOrder(81)]
        public DateTimeOffset UpdatedOn { get; set; }

        /// <summary>
        /// The entity who created the security role definition.
        /// </summary>
        [JsonPropertyName("created_by")]
        [JsonPropertyOrder(82)]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// The entity who last updated the security role definition.
        /// </summary>
        [JsonPropertyName("updated_by")]
        [JsonPropertyOrder(83)]
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Indicates whether the resource has been logically deleted.
        /// </summary>
        [JsonPropertyName("deleted")]
        [JsonPropertyOrder(100)]
        public virtual bool Deleted { get; set; } = false;
    }
}
