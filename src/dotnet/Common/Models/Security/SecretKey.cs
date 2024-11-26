namespace FoundationaLLM.Common.Models.Security
{
    /// <summary>
    /// Represents a secret key.
    /// </summary>
    public class SecretKey
    {
        /// <summary>
        /// Gets or sets the unique identifier of the secret key.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the description of the secret key.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Gets or sets the flag indicating whether the secret key is revoked.
        /// </summary>
        public required bool IsRevoked { get; set; }

        /// <summary>
        /// Gets or sets the expiration date of the secret key.
        /// </summary>
        public required DateTimeOffset ExpirationDate { get; set; }
    }
}
