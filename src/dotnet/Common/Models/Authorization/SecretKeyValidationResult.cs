using FoundationaLLM.Common.Models.Authentication;

namespace FoundationaLLM.Common.Models.Authorization
{
    /// <summary>
    /// Provides the result of a secret key validation.
    /// </summary>
    public class SecretKeyValidationResult
    {
        /// <summary>
        /// Gets or sets the flag indicating whether the secret key is valid.
        /// </summary>
        public bool Valid { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="UnifiedUserIdentity"/> virtual identity associated with the secret key.
        /// </summary>
        public required UnifiedUserIdentity VirtualIdentity { get; set; }
    }
}
