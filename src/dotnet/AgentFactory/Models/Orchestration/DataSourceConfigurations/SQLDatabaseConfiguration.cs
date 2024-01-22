using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations
{
    /// <summary>
    /// SQL Database configuration settings.
    /// </summary>
    public class SQLDatabaseConfiguration
    {
        /// <summary>
        /// The type of configuration. This value should not be changed.
        /// </summary>
        [JsonProperty("configuration_type")]
        public string ConfigurationType = "sql_database";
        /// <summary>
        /// The SQL dialect
        /// </summary>
        [JsonProperty("dialect")]
        public string? Dialect { get; set; }

        /// <summary>
        /// The database server host name.
        /// </summary>
        [JsonProperty("host")]
        public string? Host { get; set; }

        /// <summary>
        /// The port number of the database on the host.
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; set; }

        /// <summary>
        /// The name of the database on the server.
        /// </summary>
        [JsonProperty("database_name")]
        public string? DatabaseName { get; set; }

        /// <summary>
        /// The username for connecting to the database.
        /// </summary>
        [JsonProperty("username")]
        public string? Username { get; set; }

        /// <summary>
        /// The name of the secret in Key Vault from where the password can be retrieved.
        /// </summary>
        [JsonProperty("password_secret_setting_key_name")]
        public string? PasswordSecretSettingKeyName { get; set; }

        /// <summary>
        /// List of tables to allow access to in the database.
        /// </summary>
        [JsonProperty("include_tables")]
        public List<string> IncludeTables { get; set; } = new List<string>();

        /// <summary>
        /// List of tables to allow access to in the database.
        /// </summary>
        [JsonProperty("exclude_tables")]
        public List<string> ExcludeTables { get; set; } = new List<string>();

        /// <summary>
        /// The number of rows from each table to provide as examples to the language model.
        /// </summary>
        [JsonProperty("few_shot_example_count")]
        public int FewShotExampleCount { get; set; } = 0;

        /// <summary>
        /// Flag indicating whether row level security is enabled.
        /// </summary>
        [JsonProperty("row_level_security_enabled")]
        public bool RowLevelSecurityEnabled { get; set; } = false;
    }
}
