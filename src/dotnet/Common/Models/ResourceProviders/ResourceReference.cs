using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Resource reference used by resource providers to index the resources they manage.
    /// </summary>
    public class ResourceReference
    {
        /// <summary>
        /// The unique identifier of the resource.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ObjectId { get; set; }

        /// <summary>
        /// The name of the resource.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The filename of the resource.
        /// </summary>
        public required string Filename { get; set; }

        /// <summary>
        /// The type of the resource.
        /// </summary>
        public required string Type { get; set; }

        /// <summary>
        /// Indicates whether the resource has been logically deleted.
        /// </summary>
        public bool Deleted { get; set; } = false;

        /// <summary>
        /// The object type of the resource.
        /// </summary>
        /// <remarks>
        /// Derived classes should override this property to provide the type of the resource reference.
        /// </remarks>
        [JsonIgnore]
        public virtual Type ResourceType { get; } = typeof(ResourceBase);
    }
}
