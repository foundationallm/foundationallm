using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Configuration
{
    /// <summary>
    /// Provides a strongly typed configuration value with support for per-user exceptions.
    /// </summary>
    /// <typeparam name="T">The type of the configuration value</typeparam>
    public class ConfigurationValue<T>
    {
        /// <summary>
        /// Gets or sets the value of the configuration entry.
        /// </summary>
        [JsonPropertyName("value")]
        public required T Value { get; set; }

        /// <summary>
        /// Gets or sets the list of exceptions to the configuration value.
        /// </summary>
        [JsonPropertyName("value_exceptions")]
        public List<ConfigurationValueException<T>> Exceptions { get; set; } = [];

        /// <summary>
        /// Gets the value of the configuration entry for the specified user.
        /// </summary>
        /// <remarks>
        /// The method returns the user-specific value if an active exception exists for the user.
        /// If not, the method returns the default value.
        /// </remarks>
        /// <param name="userPrincipalName">The user principal name (UPN) of the user.</param>
        /// <returns>The configuration value.</returns>
        public T GetValueForUser(string userPrincipalName)
        {
            ArgumentNullException.ThrowIfNull(userPrincipalName, nameof(userPrincipalName));

            var userException = Exceptions
                .SingleOrDefault(e => e.UserPrincipalName == userPrincipalName && e.Enabled);

            return userException == null ? Value : userException.Value;
        }

        /// <summary>
        /// Deserializes a JSON string into a <see cref="ConfigurationValue{T}"/>.
        /// </summary>
        /// <param name="json">The serialized representation of the configuration value.</param>
        /// <returns>A <see cref="ConfigurationValue{T}"/> providing the configuration value.</returns>
        public static ConfigurationValue<T> Deserialize(string json) =>
            JsonSerializer.Deserialize<ConfigurationValue<T>>(json)!;
    }

    /// <summary>
    /// Provides the user-specific value of a strongly typed configuration value.
    /// </summary>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    public class ConfigurationValueException<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the configuration value is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the user principal name for the user to which the exception applies.
        /// </summary>
        [JsonPropertyName("user_principal_name")]
        public required string UserPrincipalName { get; set; }

        /// <summary>
        /// Gets or sets the value of the configuration exception entry.
        /// </summary>
        [JsonPropertyName("value")]
        public required T Value { get; set; }
    }
}
