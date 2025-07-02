using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Storage;

namespace FoundationaLLM.Context.Models.Configuration
{
    /// <summary>
    /// Provides settings for the FoundationaLLM Context API knowledge graph service.
    /// </summary>
    public class KnowledgeGraphServiceSettings
    {
        /// <summary>
        /// Gets or sets the storage settings.
        /// </summary>
        public required BlobStorageServiceSettings Storage { get; set; }
    }
}
