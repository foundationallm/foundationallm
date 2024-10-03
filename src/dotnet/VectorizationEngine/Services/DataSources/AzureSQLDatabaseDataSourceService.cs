using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Vectorization.Services.DataSources.Configuration.SQLDatabase;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services.DataSources
{
    /// <summary>
    /// Class responsible for retrieving data source data from an Azure SQL database.
    /// </summary>
    public class AzureSQLDatabaseDataSourceService
    {
        private readonly ILogger<AzureSQLDatabaseDataSourceService> _logger;
        private readonly AzureSQLDatabaseDataSource _dataSource;
        private readonly SQLDatabaseServiceSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureSQLDatabaseDataSourceService"/> class.
        /// </summary>
        /// <param name="dataSource">The data source definition.</param>
        /// <param name="settings">The Azure SQL database configuration settings.</param>
        /// <param name="loggerFactory">The factory responsible for creating loggers.</param>
        public AzureSQLDatabaseDataSourceService(
                AzureSQLDatabaseDataSource dataSource,
                SQLDatabaseServiceSettings settings,
                ILoggerFactory loggerFactory
            )
        {
            _dataSource = dataSource;
            _settings = settings;
            _logger = loggerFactory.CreateLogger<AzureSQLDatabaseDataSourceService>();
        }


        /// <summary>
        /// Executes a query to retrieve a list of multipart ids for vectorization.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>List of multipart ids to vectorize with tokens that are pipe delimited.</returns>       
        public async Task<List<string>> ExecuteMultipartQueryAsync(CancellationToken cancellationToken)
        {
            try
            { 
                using var connection = new SqlConnection(_settings.MultiPartQueryConnectionString ?? _settings.ConnectionString);
                await connection.OpenAsync(cancellationToken);
                                
                // TODO: More work to sanitize and add safety layers against harmful queries.
                using var command = new SqlCommand(_settings.MultiPartQuery, connection);

                // Execute the query and return the results as a single string list
                var results = new List<string>();

                //the first column has the multipart id, pipe delimited.
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (!reader.HasRows)
                    throw new VectorizationException($"No files matched the multipart query.");

                //iterate through the results and add them to the list
                while (await reader.ReadAsync(cancellationToken))
                {
                    results.Add(reader.GetString(0));
                }

                return results;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying for multipart id list from MultiPartQuery for data source {datasource}.", _dataSource.Name);
                throw new VectorizationException($"Error querying for multipart id list from MultiPartQuery for data source {_dataSource.Name}.", ex);
            }
        }
    }
}
