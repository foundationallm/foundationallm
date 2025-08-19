namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Provides details about the availability of a resource type in various API operations.
    /// </summary>
    public class ResourceTypeAvailability
    {
        /// <summary>
        /// Gets or sets a flag that indicates whether the resource type itself is available.
        /// </summary>
        public bool IsResourceTypeAvailable { get; set; }

        /// <summary>
        /// Gets or sets the list of available actions for the resource type.
        /// </summary>
        public HashSet<string> AvailableActions { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of available subordinate resource types.
        /// </summary>
        public Dictionary<string, ResourceTypeAvailability> AvailableSubordinateResourceTypes { get; set; } = [];
    }
}
