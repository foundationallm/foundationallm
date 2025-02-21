using FoundationaLLM.Common.Models.Cache;

namespace FoundationaLLM.Common.Models.Configuration.Authorization
{
    /// <summary>
    /// Authorization service client settings
    /// </summary>
    public record AuthorizationServiceClientSettings : CacheSettings
    {
        /// <summary>
        /// Provides the API URL of the Authorization service.
        /// </summary>
        public required string APIUrl { get; set; }

        /// <summary>
        /// Provides the API scope of the Authorization service.
        /// </summary>
        public required string APIScope { get; set; }
    }
}
