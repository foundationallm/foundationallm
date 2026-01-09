using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Configuration.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Configuration.Services
{
    /// <inheritdoc/>
#pragma warning disable CS9113 // Parameter is unread - preserved for future use
    public class ConfigurationHealthCheckService(
        IConfigurationHealthChecks healthChecks,
        IConfiguration configuration,
        ILogger<ConfigurationHealthCheckService> logger,
        IHostApplicationLifetime appLifetime)
        : IHostedService
#pragma warning restore CS9113
    {
        private readonly IHostApplicationLifetime _appLifetime = appLifetime;

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var version = configuration[EnvironmentVariables.FoundationaLLM_Version];
            var missingConfigurations = new List<string>();
            var missingKeyVaultSecrets = new List<string>();
            var missingEnvironmentVariables = new List<string>();

            // Environment Variables Check.
            try
            {
                healthChecks.ValidateEnvironmentVariables();
            }
            catch (ConfigurationValidationException ex)
            {
                if (ex.MissingEnvironmentVariables != null)
                {
                    missingEnvironmentVariables.AddRange(ex.MissingEnvironmentVariables!);
                }
            }

            // App Configuration Check.
            try
            {
                await healthChecks.ValidateConfigurationsAsync(version ?? string.Empty);
            }
            catch (ConfigurationValidationException ex)
            {
                if (ex.MissingConfigurations != null)
                {
                    missingConfigurations.AddRange(ex.MissingConfigurations!);
                }
                // Some App Configuration variables exist, but their Key Vault secrets are missing.
                if (ex.MissingKeyVaultSecrets != null)
                {
                    missingKeyVaultSecrets.AddRange(ex.MissingKeyVaultSecrets!);
                }
            }

            // Key Vault Secrets Check.
            try
            {
                await healthChecks.ValidateKeyVaultSecretsAsync(version ?? string.Empty);
            }
            catch (ConfigurationValidationException ex)
            {
                if (ex.MissingKeyVaultSecrets != null)
                {
                    missingKeyVaultSecrets.AddRange(ex.MissingKeyVaultSecrets!);
                }
            }

            // Check if any errors were accumulated across the checks
            if (missingConfigurations.Count != 0 || missingKeyVaultSecrets.Count != 0 || missingEnvironmentVariables.Count != 0)
            {
                Console.WriteLine("Configuration validation failed, shutting down:");

                foreach (var missingConfig in missingConfigurations)
                {
                    Console.WriteLine($"Missing Configuration: {missingConfig}");
                }
                foreach (var missingSecret in missingKeyVaultSecrets)
                {
                    Console.WriteLine($"Missing Key Vault Secret: {missingSecret}");
                }
                foreach (var missingVar in missingEnvironmentVariables)
                {
                    Console.WriteLine($"Missing Environment Variable: {missingVar}");
                }

                // Halt the application since critical configurations or secrets are missing.
                _appLifetime.StopApplication();
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken) =>
            await Task.CompletedTask;
    }
}
