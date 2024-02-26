using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.DataSourceConfigurations
{
    /// <summary>
    /// SQL Database configuration settings.
    /// </summary>
    public class SQLDatabaseConfiguration
    {
        /// <summary>
        /// The type of configuration. This value should not be changed.
        /// </summary>
        [JsonPropertyName("configuration_type")]
        public string ConfigurationType => "sql_database";
        /// <summary>
        /// The SQL dialect
        /// </summary>
        [JsonPropertyName("dialect")]
        public string? Dialect { get; set; }

        /// <summary>
        /// The database server host name.
        /// </summary>
        [JsonPropertyName("host")]
        public string? Host { get; set; }

        /// <summary>
        /// The port number of the database on the host.
        /// </summary>
        [JsonPropertyName("port")]
        public int Port { get; set; }

        /// <summary>
        /// The name of the database on the server.
        /// </summary>
        [JsonPropertyName("database_name")]
        public string? DatabaseName { get; set; }

        /// <summary>
        /// The username for connecting to the database.
        /// </summary>
        [JsonPropertyName("username")]
        public string? Username { get; set; }

        /// <summary>
        /// The name of the secret in Key Vault from where the password can be retrieved.
        /// </summary>
        [JsonPropertyName("password_secret_setting_key_name")]
        public string? PasswordSecretSettingKeyName { get; set; }

        /// <summary>
        /// List of tables to allow access to in the database.
        /// </summary>
        [JsonPropertyName("include_tables")]
        public List<string> IncludeTables { get; set; } = [];

        /// <summary>
        /// List of tables to allow access to in the database.
        /// </summary>
        [JsonPropertyName("exclude_tables")]
        public List<string> ExcludeTables { get; set; } = [];

        /// <summary>
        /// The number of rows from each table to provide as examples to the language model.
        /// </summary>
        [JsonPropertyName("few_shot_example_count")]
        public int FewShotExampleCount { get; set; } = 0;

        /// <summary>
        /// Flag indicating whether row level security is enabled.
        /// </summary>
        [JsonPropertyName("row_level_security_enabled")]
        public bool RowLevelSecurityEnabled { get; set; } = false;
    }
}
