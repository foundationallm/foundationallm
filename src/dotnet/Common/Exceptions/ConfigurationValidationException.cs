using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Exceptions
{
    /// <summary>
    /// Represents an exception that occurs when configuration validation fails.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="ConfigurationValidationException"/> class.
    /// </remarks>
    /// <param name="missingConfigurations">Provide a list of missing or empty app configurations.</param>
    /// <param name="missingKeyVaultSecrets">Provide a list of missing or empty Key Vault secrets.</param>
    /// <param name="missingEnvironmentVariables">Provide a list of missing environment variables.</param>
    public class ConfigurationValidationException(
        IEnumerable<string?>? missingConfigurations,
        IEnumerable<string?>? missingKeyVaultSecrets,
        IEnumerable<string?>? missingEnvironmentVariables)
        : Exception("FoundationaLLM configuration validation failed.")
    {
        /// <summary>
        /// The list of keys for missing or empty configurations.
        /// </summary>
        public IEnumerable<string?>? MissingConfigurations { get; } = missingConfigurations;
        /// <summary>
        /// The list of missing or empty Key Vault secrets.
        /// </summary>
        public IEnumerable<string?>? MissingKeyVaultSecrets { get; } = missingKeyVaultSecrets;
        /// <summary>
        /// The list of missing environment variables.
        /// </summary>
        public IEnumerable<string?>? MissingEnvironmentVariables { get; } = missingEnvironmentVariables;
    }
}
