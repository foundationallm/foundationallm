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

        /// <summary>
        /// Determines whether two specified instances of <see cref="ResourceReference"/> are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ResourceReference"/> to compare.</param>
        /// <param name="right">The second <see cref="ResourceReference"/> to compare.</param>
        /// <returns>true if the two <see cref="ResourceReference"/> instances are equal; otherwise, false.</returns>
        public static bool operator ==(ResourceReference left, ResourceReference right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left is null || right is null)
                return false;

            return left.ObjectId == right.ObjectId &&
                   left.Name == right.Name &&
                   left.Filename == right.Filename &&
                   left.Type == right.Type &&
                   left.Deleted == right.Deleted;
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="ResourceReference"/> are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="ResourceReference"/> to compare.</param>
        /// <param name="right">The second <see cref="ResourceReference"/> to compare.</param>
        /// <returns>true if the two <see cref="ResourceReference"/> instances are not equal; otherwise, false.</returns>
        public static bool operator !=(ResourceReference left, ResourceReference right) =>
            !(left == right);

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="ResourceReference"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current <see cref="ResourceReference"/>.</param>
        /// <returns>true if the specified object is equal to the current <see cref="ResourceReference"/>; otherwise, false.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is ResourceReference other)
            {
                return this == other;
            }
            return false;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current <see cref="ResourceReference"/>.</returns>
        public override int GetHashCode() =>
            HashCode.Combine(ObjectId, Name, Filename, Type, Deleted);

    }
}
