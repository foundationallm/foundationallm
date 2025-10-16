using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Filters resources based on the specified criteria.
    /// </summary>
    public class ResourceFilter
    {
        /// <summary>
        /// Gets or sets a value that specifies whether the default resource should be retrieved or not.
        /// </summary>
        /// <remarks>
        /// If set, this value has precedence over the <see cref="ObjectIDs"/> property.
        /// If not set, the <see cref="ObjectIDs"/> property is used to filter resources.
        /// </remarks>
        [JsonPropertyName("default")]
        public bool? DefaultResource { get; set; }

        /// <summary>
        /// Gets or sets a list of object IDs to filter resources.
        /// </summary>
        /// <remarks>
        /// The <see cref="DefaultResource"/> property has precendece over this property.
        /// If the <see cref="DefaultResource"/> property is set, this property is ignored.
        /// </remarks>
        [JsonPropertyName("object_ids")]
        public List<string>? ObjectIDs { get; set; }

        /// <summary>
        /// Gets or sets the name of the resource to filter by.
        /// </summary>
        /// <remarks>
        /// The value can be a full name, a partial name, or a name pattern (depending on the support
        /// provided by the resource provider).
        /// </remarks>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
