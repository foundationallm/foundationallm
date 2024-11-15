namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Identifies a specific resource type instance.
    /// </summary>
    /// <param name="ResourceTypeName">The name of the resource type.</param>
    /// <param name="ResourceType">The object type of the resource type.</param>
    public record ResourceTypeInstance(
        string ResourceTypeName,
        Type ResourceType)
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
        /// Checks whether the current instance includes another <see cref="ResourceTypeInstance"/> instance.
        /// </summary>
        /// <param name="other">The <see cref="ResourceTypeInstance"/> to check for inclusion.</param>
        /// <returns>True if the current instance includes the other instance.</returns>
        public bool Includes(ResourceTypeInstance? other)
        {
            if (other == null)
                return false;

            // Identical resource type instances
            if (ReferenceEquals(this, other))
                return true;

            // Resource type instances with different resource types
            if (!ResourceTypeName.Equals(other.ResourceTypeName))
                return false;

            if (this.ResourceId == null)
            {
                // The other resource type instance refers to a specific resource,
                // so it's more specific than the current one.
                if (other.ResourceId != null)
                    return false;

                if (this.Action == null)
                {
                    // If the other action is not null, it will be more specific than the current one
                    return other.Action == null;
                }
                else
                {
                    // If the other action is not null and different than the current, then there is a difference
                    return (other.Action == null) || this.Action.Equals(other.Action);
                }    
            }
            else
            {
                if (this.Action == null)
                {
                    // If the other action is not null, it will be more specific than the current one.
                    // If the other resource id is not null and different from the current one, then there is a difference
                    return (other.Action == null) && ((other.ResourceId == null) || this.ResourceId.Equals(other.ResourceId));
                }
                else
                {
                    if (other.Action == null)
                    {
                        // If the other resource id exists and is different from the current resource id, then there is a difference
                        return (other.ResourceId == null) || this.ResourceId.Equals(other.ResourceId);
                    }
                    else
                    {
                        return this.Action.Equals(other.Action) && (other.ResourceId != null) && this.ResourceId.Equals(other.ResourceId);
                    }
                }
            };
        }
    }
}
