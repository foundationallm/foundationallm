using System.Text.Json.Serialization;
using FoundationaLLM.DataSource.Constants;

namespace FoundationaLLM.DataSource.Models
{
    /// <summary>
    /// Azure SQL Database data source.
    /// </summary>
    public class AzureSQLDatabaseDataSource : DataSourceBase
    {
        /// <summary>
        /// The list of tables from the database. The format is [schema].[table_name].
        /// </summary>
        [JsonPropertyName("tables")]
        public List<string> Tables { get; set; } = [];

        /// <summary>
        /// Creates a new instance of the <see cref="AzureSQLDatabaseDataSource"/> data source.
        /// </summary>
        public AzureSQLDatabaseDataSource() =>
            Type = DataSourceTypes.AzureSQLDatabase;
    }
}
