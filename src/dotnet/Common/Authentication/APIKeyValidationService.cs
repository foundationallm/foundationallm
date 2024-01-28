using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Authentication
{
    /// <summary>
    /// Implements the <see cref="IAPIKeyValidationService"/> interface.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the <see cref="APIKeyValidationService"/> class.
    /// </remarks>
    /// <param name="options">otions for the deployed API key validation service.</param>
    public class APIKeyValidationService(
        IOptions<APIKeyValidationSettings> options) : IAPIKeyValidationService
    {
        private readonly APIKeyValidationSettings _settings = options.Value;

        /// <summary>
        /// Checks if an API key is valid or not.
        /// </summary>
        /// <param name="apiKey">The API key to be checked.</param>
        /// <returns>A boolean value representing the validity of the API key.</returns>
        public bool IsValid(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return false;

            string? validApiKey = _settings.APIKey;

            if (validApiKey == null || validApiKey != apiKey)
                return false;

            return true;
        }
    }
}
