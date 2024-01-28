using FoundationaLLM.Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Models.Configuration
{
    /// <summary>
    /// Provides configuration settings to initialize a vectorization state service.
    /// </summary>
    public class VectorizationStateServiceSettings
    {
        /// <summary>
        /// The settings for connecting to the underlying blob storage.
        /// </summary>
        public required BlobStorageServiceSettings Storage { get; set; }

        /// <summary>
        /// The name of the container where the underlying persistence service stores vectorization state.
        /// </summary>
        public required string StorageContainerName { get; set; }
    }
}
