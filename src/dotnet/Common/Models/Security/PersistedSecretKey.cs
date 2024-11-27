using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Common.Models.Security
{
    /// <summary>
    /// Represents a persisted secret key.
    /// </summary>
    public class PersistedSecretKey : SecretKey
    {
        /// <summary>
        /// Gets or sets the salt used in the computation of the hash.
        /// </summary>
        /// <remarks>
        /// The salt is a random value that is used to protect against dictionary attacks.
        /// It is generated once and stored with the secret key in a Base58 encoded format.
        /// </remarks>
        public required string Salt { get; set; }

        /// <summary>
        /// Gets or sets the hash of the secret key.
        /// </summary>
        /// <remarks>
        /// The hash is computed by applying the hashing algorithm to the secret key and the salt.
        /// </remarks>
        public required string Hash { get; set; }
    }
}
