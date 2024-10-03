namespace FoundationaLLM.Vectorization.Services.DataSources.Configuration.SQLDatabase
{
    /// <summary>
    /// Provides configuration settings to initialize an SQL database service.
    /// </summary>
    public record SQLDatabaseServiceSettings
    {
        /// <summary>
        /// The connection string used for authentication to the SQL database with the data to be vectorized.
        /// </summary>
        public required string ConnectionString { get; set; }
              
        /// <summary>
        /// Optional: When not vectorizing an entire table, this query is used to retrieve the data to be vectorized.
        /// </summary>
        public string? MultiPartQuery { get; set; }

        /// <summary>
        /// Optional: The connection string used for authentication to the SQL database when using a multi-part query if it is different from ConnectionString.
        /// </summary>
        public string? MultiPartQueryConnectionString { get; set; }
    }
}
