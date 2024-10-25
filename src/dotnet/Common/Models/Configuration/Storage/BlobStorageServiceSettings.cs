using FoundationaLLM.Common.Constants.Authentication;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Configuration.Storage
{
    /// <summary>
    /// Provides configuration settings to initialize a blob storage service.
    /// </summary>
    public record BlobStorageServiceSettings
    {
        /// <summary>
        /// A <see cref="AuthenticationTypes"/> value indicating the type of authentication used.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required AuthenticationTypes AuthenticationType { get; set; }

        /// <summary>
        /// The name of the blob storage account.
        /// </summary>
        public string? AccountName { get; set; }

        /// <summary>
        /// The account key used for authentication.
        /// This value should be set only if AuthenticationType has a value of AccountKey.
        /// </summary>
        public string? AccountKey { get; set; }

        /// <summary>
        /// The connection string used for authentication.
        /// This value should be set only if AuthenticationType has a value of ConnectionString.
        /// </summary>
        public string? ConnectionString { get; set; }
    }
}
