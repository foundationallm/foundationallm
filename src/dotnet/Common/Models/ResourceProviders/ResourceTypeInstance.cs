using System.Diagnostics.CodeAnalysis;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Identifies a specific resource type instance.
    /// </summary>
    /// <param name="ResourceType">The name of the resource type.</param>
    public record ResourceTypeInstance(
        string ResourceType)
    {
        /// <summary>
        /// An optional resource type instance unique identifier.
        /// </summary>
        public string? ResourceId;

        /// <summary>
        /// An optional action to be executed on the resource instance.
        /// </summary>
        public string? Action;

        /// <summary>
        /// Checks whether the current instance is equal to another <see cref="ResourceTypeInstance"/> instance.
        /// </summary>
        /// <param name="other">The <see cref="ResourceTypeInstance"/> to check for equality.</param>
        /// <returns>True if the two instances are equal.</returns>
        public bool EqualTo(ResourceTypeInstance? other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (ResourceId == null || other.ResourceId == null)
                return false;
            if (Action == null || other.Action == null)
                return false;

            return
                ResourceType.Equals(other.ResourceType)
                && ResourceId.Equals(other.ResourceId)
                && Action.Equals(other.Action);
        }
    }
}
