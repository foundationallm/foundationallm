using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Provides the details necessary to upgrade the schema of a resource reference list.
    /// </summary>
    /// <typeparam name="T">The type of resource reference kept in the store.</typeparam>
    public class ResourceReferenceListSchemaUpgrade<T> where T : ResourceReference
    {
        /// <summary>
        /// Gets or sets the version of the schema to upgrade to.
        /// </summary>
        public int SchemaVersion { get; set; }

        /// <summary>
        /// Gets or sets the action that performs the upgrade on a resource reference.
        /// </summary>
        public required Func<T, IStorageService, ILogger, Task> ResourceReferenceUpgradeAction { get; set; }
    }
}
