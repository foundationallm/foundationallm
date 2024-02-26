using FoundationaLLM.Common.Exceptions;

namespace FoundationaLLM.Configuration.Interfaces
{
    /// <summary>
    /// Provides health checks for the application's configuration settings.
    /// </summary>
    public interface IConfigurationHealthChecks
    {
        /// <summary>
        /// Validates the application's configuration settings.
        /// </summary>
        /// <param name="version">The current app version.</param>
        /// <returns></returns>
        Task ValidateConfigurationsAsync(string version);

        /// <summary>
        /// Validates the application's Key Vault secrets.
        /// </summary>
        /// <param name="version">The current app version.</param>
        /// <returns></returns>
        /// <exception cref="ConfigurationValidationException"></exception>
        Task ValidateKeyVaultSecretsAsync(string version);

        /// <summary>
        /// Validates the application's environment variables.
        /// </summary>
        void ValidateEnvironmentVariables();
    }
}
