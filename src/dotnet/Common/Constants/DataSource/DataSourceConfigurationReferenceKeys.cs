namespace FoundationaLLM.Common.Constants.DataSource
{
    /// <summary>
    /// Defines the keys for data source configuration references.
    /// </summary>
    public static class DataSourceConfigurationReferenceKeys
    {
        /// <summary>
        /// The client identifier of the application registration.
        /// </summary>
        public const string ClientId = "ClientId";

        /// <summary>
        /// The Entra ID tenant identifier where the application registration is located.
        /// </summary>
        public const string TenantId = "TenantId";

        /// <summary>
        /// The name of the X.509 certificate used for authentication.
        /// </summary>
        public const string CertificateName = "CertificateName";

        /// <summary>
        /// The URL of the Azure Key Vault where the X.509 certificate is stored.
        /// </summary>
        public const string KeyVaultUrl = "KeyVaultURL";
    }
}
