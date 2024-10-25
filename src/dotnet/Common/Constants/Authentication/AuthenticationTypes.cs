namespace FoundationaLLM.Common.Constants.Authentication
{
    /// <summary>
    /// Authentication types for API Endpoints
    /// </summary>
    public enum AuthenticationTypes
    {
        /// <summary>
        /// Unknown authentication type.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Azure managed identity authentication type.
        /// </summary>
        AzureIdentity,

        /// <summary>
        /// API key authentication type.
        /// </summary>
        APIKey,

        /// <summary>
        /// Connection string authentication type.
        /// </summary>
        ConnectionString,

        /// <summary>
        /// Account key authentication type.
        /// </summary>
        AccountKey
    }
}
