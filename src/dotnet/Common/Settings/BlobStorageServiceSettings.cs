using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Settings
{
    /// <summary>
    /// Provides configuration settings to initialize a blob storage service.
    /// </summary>
    public record BlobStorageServiceSettings
    {
        /// <summary>
        /// The name of the blob storage account.
        /// </summary>
        public required string AccountName { get; set; }

        /// <summary>
        /// A <see cref="BlobStorageAuthenticationTypes"/> value indicating the type of authentication used.
        /// </summary>
        public required BlobStorageAuthenticationTypes AuthenticationType { get; set; }

        /// <summary>
        /// The account key used for authentication.
        /// This value should be set only if AuthenticationType has a value of AccountKey.
        /// </summary>
        public string? AccountKey {  get; set; }

        /// <summary>
        /// The connection string used for authentication.
        /// This value should be set only if AuthenticationType has a value of ConnectionString.
        /// </summary>
        public string? ConnectionString { get; set; }
    }
}
