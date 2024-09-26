namespace FoundationaLLM.Common.Constants.Authorization
{
    /// <summary>
    /// Provides the names of the core authorizable operations.
    /// </summary>
    public static class AuthorizableOperations
    {
        /// <summary>
        /// Read resources.
        /// </summary>
        public const string Read = "read";

        /// <summary>
        /// Create or update resources.
        /// </summary>
        public const string Write = "write";

        /// <summary>
        /// Delete or purge resources.
        /// </summary>
        public const string Delete = "delete";
    }
}
