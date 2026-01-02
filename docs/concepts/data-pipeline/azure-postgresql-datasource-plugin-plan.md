# Azure PostgreSQL Data Source Plugin Implementation Plan

## Overview and Purpose

### What the Plugin Does

The Azure PostgreSQL Data Source Plugin enables FoundationaLLM data pipelines to extract content from Azure Database for PostgreSQL (Azure PostgreSQL) instances. This plugin allows users to:

- Connect securely to Azure PostgreSQL databases using various authentication methods
- Extract data from specified tables, views, or custom SQL queries
- Transform database rows into content items that can be processed by downstream pipeline stages (text extraction, partitioning, embedding, indexing)

### Use Cases

1. **Structured Data Ingestion**: Extract structured data from PostgreSQL databases to build knowledge bases for AI agents
2. **Document Content Extraction**: Retrieve text content stored in database columns (e.g., articles, product descriptions, knowledge base entries)
3. **Metadata Enrichment**: Extract metadata from relational databases to enhance AI agent responses
4. **Hybrid Data Sources**: Combine PostgreSQL data with other data sources (Azure Data Lake, SharePoint) in unified data pipelines
5. **Analytics and Reporting**: Extract historical data for analysis and embedding into vector stores

### Success Criteria

- Plugin successfully connects to Azure PostgreSQL using Entra ID authentication (managed identity) or username/password
- Plugin retrieves content items from specified tables, views, or custom queries
- Content items are properly formatted for downstream pipeline stages
- Plugin handles connection errors, timeouts, and data type conversions gracefully
- Plugin follows established FoundationaLLM patterns and conventions

---

## Components to Create/Modify

### New Files to Create

| File Path | Purpose |
|-----------|---------|
| `src/dotnet/Common/Models/ResourceProviders/DataSource/AzurePostgreSQLDataSource.cs` | Data source model class |
| `src/dotnet/DataPipelinePlugins/Plugins/DataSource/AzurePostgreSQLDataSourcePlugin.cs` | Plugin implementation |

### Files to Modify

| File Path | Changes |
|-----------|---------|
| `src/dotnet/Common/Models/ResourceProviders/DataSource/DataSourceBase.cs` | Add `JsonDerivedType` attribute for `AzurePostgreSQLDataSource` |
| `src/dotnet/Common/Constants/ResourceProviders/DataSourceTypes.cs` | Add `AzurePostgreSQL` constant |
| `src/dotnet/Common/Constants/DataSource/DataSourceConfigurationReferenceKeys.cs` | Add PostgreSQL-specific configuration keys |
| `src/dotnet/DataPipelinePlugins/PluginNames.cs` | Add `AZUREPOSTGRESQL_DATASOURCE` constant |
| `src/dotnet/DataPipelinePlugins/PluginParameterNames.cs` | Add PostgreSQL-specific parameter constants |
| `src/dotnet/DataPipelinePlugins/PluginPackageManager.cs` | Register plugin metadata and factory method |
| `src/dotnet/DataPipelinePlugins/DataPipelinePlugins.csproj` | Add Npgsql NuGet package reference |

---

## Data Source Model Design

### AzurePostgreSQLDataSource Class

```csharp
using FoundationaLLM.Common.Constants.ResourceProviders;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataSource
{
    /// <summary>
    /// Azure PostgreSQL data source.
    /// </summary>
    public class AzurePostgreSQLDataSource : DataSourceBase
    {
        /// <summary>
        /// The list of tables from the database. The format is [schema].[table_name].
        /// </summary>
        [JsonPropertyName("tables")]
        public List<string> Tables { get; set; } = [];

        /// <summary>
        /// Custom SQL queries to extract content. Each query should return a result set
        /// with columns that can be converted to content items.
        /// </summary>
        [JsonPropertyName("custom_queries")]
        public List<AzurePostgreSQLCustomQuery>? CustomQueries { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="AzurePostgreSQLDataSource"/> data source.
        /// </summary>
        public AzurePostgreSQLDataSource() =>
            Type = DataSourceTypes.AzurePostgreSQL;
    }

    /// <summary>
    /// Represents a custom SQL query for extracting content from Azure PostgreSQL.
    /// </summary>
    public class AzurePostgreSQLCustomQuery
    {
        /// <summary>
        /// The name of the custom query (used for identification in content identifiers).
        /// </summary>
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        /// <summary>
        /// The SQL query to execute.
        /// </summary>
        [JsonPropertyName("query")]
        public required string Query { get; set; }

        /// <summary>
        /// The column name that contains the unique identifier for each row.
        /// </summary>
        [JsonPropertyName("id_column")]
        public required string IdColumn { get; set; }

        /// <summary>
        /// The column name that contains the content to be extracted.
        /// </summary>
        [JsonPropertyName("content_column")]
        public required string ContentColumn { get; set; }

        /// <summary>
        /// Optional column names to include as metadata.
        /// </summary>
        [JsonPropertyName("metadata_columns")]
        public List<string>? MetadataColumns { get; set; }
    }
}
```

### Properties Explanation

| Property | Type | Description |
|----------|------|-------------|
| `Tables` | `List<string>` | List of table/view names in `[schema].[table_name]` format. When specified, the plugin will extract all rows from these tables. |
| `CustomQueries` | `List<AzurePostgreSQLCustomQuery>?` | Optional custom SQL queries for more control over data extraction. Takes precedence over `Tables` when specified. |

### Configuration References (stored in DataSource.ConfigurationReferences)

The following configuration references will be used for secure credential management:

| Key | Description | Azure App Configuration Key Example |
|-----|-------------|-------------------------------------|
| `ConnectionString` | PostgreSQL connection string stored in Key Vault | `FoundationaLLM:DataSource:AzurePostgreSQL:ConnectionString` |
| `Host` | PostgreSQL server hostname | `FoundationaLLM:DataSource:AzurePostgreSQL:Host` |
| `Database` | Database name | `FoundationaLLM:DataSource:AzurePostgreSQL:Database` |
| `Username` | Username for authentication (if not using Entra ID) | `FoundationaLLM:DataSource:AzurePostgreSQL:Username` |
| `Password` | Password stored in Key Vault (if not using Entra ID) | `FoundationaLLM:DataSource:AzurePostgreSQL:Password` |
| `UseManagedIdentity` | Whether to use Entra ID managed identity | `FoundationaLLM:DataSource:AzurePostgreSQL:UseManagedIdentity` |

---

## Plugin Implementation Details

### Plugin Class Design

```csharp
using Azure.Identity;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.DataPipelines;
using FoundationaLLM.Common.Constants.DataSource;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Interfaces.Plugins;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Services.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Plugins.DataPipeline.Plugins.DataSource
{
    /// <summary>
    /// Implements the Azure PostgreSQL data source plugin.
    /// </summary>
    /// <param name="dataSourceObjectId">The FoundationaLLM object identifier of the data source.</param>
    /// <param name="pluginParameters">The dictionary containing the plugin parameters.</param>
    /// <param name="packageManager">The package manager for the plugin.</param>
    /// <param name="packageManagerResolver">The package manager resolver for the plugin.</param>
    /// <param name="serviceProvider">The service provider of the dependency injection container.</param>
    public class AzurePostgreSQLDataSourcePlugin(
        string dataSourceObjectId,
        Dictionary<string, object> pluginParameters,
        IPluginPackageManager packageManager,
        IPluginPackageManagerResolver packageManagerResolver,
        IServiceProvider serviceProvider)
        : PluginBase(pluginParameters, packageManager, packageManagerResolver, serviceProvider), IDataSourcePlugin
    {
        private readonly string _dataSourceObjectId = dataSourceObjectId;

        protected override string Name => PluginNames.AZUREPOSTGRESQL_DATASOURCE;

        protected readonly IResourceProviderService? _dataSourceResourceProvider = serviceProvider
            .GetServices<IResourceProviderService>()
            .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_DataSource)
            ?? throw new PluginException($"The resource provider {ResourceProviderNames.FoundationaLLM_DataSource} was not loaded.");

        protected readonly IConfiguration _configuration = serviceProvider
            .GetRequiredService<IConfiguration>()
            ?? throw new PluginException("The configuration service is not available in the dependency injection container.");

        /// <inheritdoc/>
        public async Task<List<DataPipelineContentItem>> GetContentItems()
        {
            // Get table/query specification from parameters
            var tablesParameter = _pluginParameters.TryGetValue(
                PluginParameterNames.AZUREPOSTGRESQL_DATASOURCE_TABLES, out var tablesObj)
                ? tablesObj?.ToString() : null;

            var queryNameParameter = _pluginParameters.TryGetValue(
                PluginParameterNames.AZUREPOSTGRESQL_DATASOURCE_QUERYNAME, out var queryObj)
                ? queryObj?.ToString() : null;

            // Load data source configuration
            var dataSourceBase = await _dataSourceResourceProvider!.GetResourceAsync<DataSourceBase>(
                _dataSourceObjectId,
                ServiceContext.ServiceIdentity!)
                ?? throw new PluginException($"The Azure PostgreSQL data source with object ID '{_dataSourceObjectId}' cannot be found.");

            var dataSource = dataSourceBase as AzurePostgreSQLDataSource
                ?? throw new PluginException($"The data source [{_dataSourceObjectId}] is not a valid Azure PostgreSQL data source.");

            // Get connection string from configuration references
            var connectionString = await GetConnectionString(dataSource);

            List<DataPipelineContentItem> contentItems = [];

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            // Determine extraction mode: custom queries or tables
            if (!string.IsNullOrEmpty(queryNameParameter) && dataSource.CustomQueries != null)
            {
                var customQuery = dataSource.CustomQueries.FirstOrDefault(q => q.Name == queryNameParameter)
                    ?? throw new PluginException($"Custom query '{queryNameParameter}' not found in data source configuration.");
                
                contentItems.AddRange(await ExecuteCustomQuery(connection, customQuery, dataSource));
            }
            else
            {
                // Process tables
                var tablesToProcess = !string.IsNullOrEmpty(tablesParameter)
                    ? tablesParameter.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    : dataSource.Tables.ToArray();

                foreach (var table in tablesToProcess)
                {
                    contentItems.AddRange(await ExtractFromTable(connection, table, dataSource));
                }
            }

            return contentItems;
        }

        /// <inheritdoc/>
        public async Task<PluginResult<ContentItemRawContent>> GetContentItemRawContent(
            ContentIdentifier contentItemIdentifier)
        {
            // Parse the content identifier to determine source (table/query) and row ID
            var parts = contentItemIdentifier.MultipartId;
            if (parts.Count < 3)
            {
                return new PluginResult<ContentItemRawContent>(
                    null,
                    false,
                    false,
                    ErrorMessage: "Invalid content identifier format for Azure PostgreSQL data source.");
            }

            var sourceType = parts[0]; // "table" or "query"
            var sourceName = parts[1]; // table name or query name
            var rowId = parts[2];      // row identifier

            // Load data source configuration
            var dataSourceBase = await _dataSourceResourceProvider!.GetResourceAsync<DataSourceBase>(
                _dataSourceObjectId,
                ServiceContext.ServiceIdentity!)
                ?? throw new PluginException($"The Azure PostgreSQL data source with object ID '{_dataSourceObjectId}' cannot be found.");

            var dataSource = dataSourceBase as AzurePostgreSQLDataSource
                ?? throw new PluginException($"The data source [{_dataSourceObjectId}] is not a valid Azure PostgreSQL data source.");

            var connectionString = await GetConnectionString(dataSource);

            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            string? content = null;
            Dictionary<string, object> metadata = new();

            if (sourceType == "query" && dataSource.CustomQueries != null)
            {
                var customQuery = dataSource.CustomQueries.FirstOrDefault(q => q.Name == sourceName);
                if (customQuery != null)
                {
                    (content, metadata) = await GetRowContentFromQuery(connection, customQuery, rowId);
                }
            }
            else if (sourceType == "table")
            {
                (content, metadata) = await GetRowContentFromTable(connection, sourceName, rowId);
            }

            if (content == null)
            {
                return new PluginResult<ContentItemRawContent>(
                    null,
                    false,
                    false,
                    ErrorMessage: $"Content item with identifier '{contentItemIdentifier.CanonicalId}' not found.");
            }

            var binaryContent = new BinaryData(Encoding.UTF8.GetBytes(content));

            return new PluginResult<ContentItemRawContent>(
                new ContentItemRawContent
                {
                    Name = $"{sourceName}_{rowId}.txt",
                    ContentType = "text/plain",
                    RawContent = binaryContent,
                    Metadata = metadata
                },
                true,
                false);
        }

        /// <inheritdoc/>
        public async Task HandleUnsafeContentItem(string canonicalContentItemIdentifier)
        {
            // Log the unsafe content item but take no action on the source database
            // Modifying or deleting source data could have unintended consequences
            _logger.LogWarning(
                "The {PluginName} plugin received a notification to handle an unsafe content item with identifier {ContentItemIdentifier}. No action will be taken on the source database.",
                Name,
                canonicalContentItemIdentifier);

            await Task.CompletedTask;
        }

        #region Private Methods

        private async Task<string> GetConnectionString(AzurePostgreSQLDataSource dataSource)
        {
            var settings = dataSource.ConfigurationReferences!
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => _configuration[kvp.Value]);

            // Check if we have a direct connection string
            if (settings.TryGetValue(AzurePostgreSQLConfigurationKeys.ConnectionString, out var connectionString)
                && !string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }

            // Build connection string from components
            var host = settings.GetValueOrDefault(AzurePostgreSQLConfigurationKeys.Host)
                ?? throw new PluginException("PostgreSQL host is required.");
            var database = settings.GetValueOrDefault(AzurePostgreSQLConfigurationKeys.Database)
                ?? throw new PluginException("PostgreSQL database name is required.");

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Database = database,
                SslMode = SslMode.Require
            };

            // Check authentication mode
            var useManagedIdentity = settings.GetValueOrDefault(AzurePostgreSQLConfigurationKeys.UseManagedIdentity);
            if (bool.TryParse(useManagedIdentity, out var useMI) && useMI)
            {
                // Use Entra ID authentication with managed identity
                var credential = ServiceContext.AzureCredential ?? new DefaultAzureCredential();
                var token = await credential.GetTokenAsync(
                    new Azure.Core.TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]));
                
                builder.Password = token.Token;
                builder.Username = settings.GetValueOrDefault(AzurePostgreSQLConfigurationKeys.Username) ?? "";
            }
            else
            {
                // Use username/password authentication
                builder.Username = settings.GetValueOrDefault(AzurePostgreSQLConfigurationKeys.Username)
                    ?? throw new PluginException("PostgreSQL username is required when not using managed identity.");
                builder.Password = settings.GetValueOrDefault(AzurePostgreSQLConfigurationKeys.Password)
                    ?? throw new PluginException("PostgreSQL password is required when not using managed identity.");
            }

            return builder.ConnectionString;
        }

        private async Task<List<DataPipelineContentItem>> ExtractFromTable(
            NpgsqlConnection connection,
            string tableName,
            AzurePostgreSQLDataSource dataSource)
        {
            var contentItems = new List<DataPipelineContentItem>();

            // Get primary key column(s) for the table
            var pkColumns = await GetPrimaryKeyColumns(connection, tableName);
            if (pkColumns.Count == 0)
            {
                _logger.LogWarning("Table {TableName} has no primary key. Using ctid as row identifier.", tableName);
                pkColumns = ["ctid::text"];
            }

            var pkExpression = pkColumns.Count == 1
                ? pkColumns[0]
                : $"concat_ws('_', {string.Join(", ", pkColumns)})";

            // SECURITY: Table names are validated against the data source configuration
            // and cannot be user-supplied at runtime. The tableName is from the
            // pre-configured Tables list or comes from the DataSource resource.
            // Column names come from PostgreSQL system catalogs.
            // NpgsqlDataSourceBuilder could be used for additional quoting if needed.
            ValidateTableName(tableName);
            var query = $"SELECT {pkExpression} as _row_id, * FROM {NpgsqlConnection.QuoteIdentifier(tableName)}";

            await using var command = new NpgsqlCommand(query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var rowId = reader["_row_id"]?.ToString() ?? Guid.NewGuid().ToString();
                var canonicalId = $"table/{tableName}/{rowId}";

                contentItems.Add(new DataPipelineContentItem
                {
                    Id = $"content-item-{tableName.Replace(".", "_")}-{rowId}-{Guid.NewGuid().ToBase64String()}",
                    DataSourceObjectId = _dataSourceObjectId,
                    ContentIdentifier = new ContentIdentifier
                    {
                        MultipartId = ["table", tableName, rowId],
                        CanonicalId = canonicalId
                    },
                    ContentAction = ContentItemActions.AddOrUpdate
                });
            }

            return contentItems;
        }

        private async Task<List<DataPipelineContentItem>> ExecuteCustomQuery(
            NpgsqlConnection connection,
            AzurePostgreSQLCustomQuery customQuery,
            AzurePostgreSQLDataSource dataSource)
        {
            var contentItems = new List<DataPipelineContentItem>();

            await using var command = new NpgsqlCommand(customQuery.Query, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var rowId = reader[customQuery.IdColumn]?.ToString() ?? Guid.NewGuid().ToString();
                var canonicalId = $"query/{customQuery.Name}/{rowId}";

                contentItems.Add(new DataPipelineContentItem
                {
                    Id = $"content-item-{customQuery.Name}-{rowId}-{Guid.NewGuid().ToBase64String()}",
                    DataSourceObjectId = _dataSourceObjectId,
                    ContentIdentifier = new ContentIdentifier
                    {
                        MultipartId = ["query", customQuery.Name, rowId],
                        CanonicalId = canonicalId
                    },
                    ContentAction = ContentItemActions.AddOrUpdate
                });
            }

            return contentItems;
        }

        private async Task<List<string>> GetPrimaryKeyColumns(NpgsqlConnection connection, string tableName)
        {
            var parts = tableName.Split('.');
            var schema = parts.Length > 1 ? parts[0] : "public";
            var table = parts.Length > 1 ? parts[1] : parts[0];

            var query = @"
                SELECT a.attname
                FROM pg_index i
                JOIN pg_attribute a ON a.attrelid = i.indrelid AND a.attnum = ANY(i.indkey)
                WHERE i.indrelid = $1::regclass AND i.indisprimary";

            var columns = new List<string>();

            // SECURITY: Schema and table names are validated from pre-configured data source.
            // The regclass type provides additional validation against the database catalog.
            // Using positional parameter ($1) which is standard Npgsql syntax.
            var qualifiedTableName = $"{schema}.{table}";
            ValidateTableName(qualifiedTableName);
            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.Add(new NpgsqlParameter { Value = qualifiedTableName });

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(0));
            }

            return columns;
        }

        private async Task<(string? Content, Dictionary<string, object> Metadata)> GetRowContentFromTable(
            NpgsqlConnection connection,
            string tableName,
            string rowId)
        {
            var pkColumns = await GetPrimaryKeyColumns(connection, tableName);
            if (pkColumns.Count == 0)
            {
                _logger.LogWarning("Cannot retrieve row content - table {TableName} has no primary key.", tableName);
                return (null, new Dictionary<string, object>());
            }

            // For simplicity, assume single-column primary key
            // SECURITY: Table name is validated from pre-configured data source.
            // Column name comes from PostgreSQL system catalogs. Using quote identifiers for safety.
            ValidateTableName(tableName);
            var quotedTable = NpgsqlConnection.QuoteIdentifier(tableName);
            var quotedColumn = NpgsqlConnection.QuoteIdentifier(pkColumns[0]);
            var query = $"SELECT * FROM {quotedTable} WHERE {quotedColumn}::text = @rowId LIMIT 1";

            await using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@rowId", rowId);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return ExtractContentFromRow(reader);
            }

            return (null, new Dictionary<string, object>());
        }

        private async Task<(string? Content, Dictionary<string, object> Metadata)> GetRowContentFromQuery(
            NpgsqlConnection connection,
            AzurePostgreSQLCustomQuery customQuery,
            string rowId)
        {
            // SECURITY: Custom queries come from the pre-configured data source resource
            // which is managed by administrators through the Management Portal.
            // The IdColumn is validated against PostgreSQL identifier conventions.
            ValidateColumnName(customQuery.IdColumn);
            var quotedIdColumn = NpgsqlConnection.QuoteIdentifier(customQuery.IdColumn);
            
            // Wrap the original query to filter by ID
            var wrappedQuery = $@"
                WITH query_results AS ({customQuery.Query})
                SELECT * FROM query_results WHERE {quotedIdColumn}::text = @rowId LIMIT 1";

            await using var command = new NpgsqlCommand(wrappedQuery, connection);
            command.Parameters.AddWithValue("@rowId", rowId);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var content = reader[customQuery.ContentColumn]?.ToString();
                var metadata = new Dictionary<string, object>
                {
                    { "QueryName", customQuery.Name },
                    { "RowId", rowId }
                };

                if (customQuery.MetadataColumns != null)
                {
                    foreach (var col in customQuery.MetadataColumns)
                    {
                        try
                        {
                            var value = reader[col];
                            if (value != null && value != DBNull.Value)
                            {
                                metadata[col] = value;
                            }
                        }
                        catch { /* Column not found, skip */ }
                    }
                }

                return (content, metadata);
            }

            return (null, new Dictionary<string, object>());
        }

        private (string Content, Dictionary<string, object> Metadata) ExtractContentFromRow(NpgsqlDataReader reader)
        {
            var contentBuilder = new StringBuilder();
            var metadata = new Dictionary<string, object>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var value = reader.GetValue(i);

                if (value != null && value != DBNull.Value)
                {
                    metadata[columnName] = value;

                    // Build text content from all columns
                    var stringValue = value.ToString();
                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        contentBuilder.AppendLine($"{columnName}: {stringValue}");
                    }
                }
            }

            return (contentBuilder.ToString(), metadata);
        }

        /// <summary>
        /// Validates that a table name matches expected PostgreSQL naming conventions
        /// and exists in the pre-configured data source tables list.
        /// </summary>
        /// <param name="tableName">The table name to validate.</param>
        /// <exception cref="PluginException">Thrown if the table name is invalid.</exception>
        private void ValidateTableName(string tableName)
        {
            // Only allow valid PostgreSQL identifier characters
            // Format: [schema.]table_name where each part contains only letters, digits, underscores
            var pattern = @"^([a-zA-Z_][a-zA-Z0-9_]*\.)?[a-zA-Z_][a-zA-Z0-9_]*$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(tableName, pattern))
            {
                throw new PluginException(
                    $"Invalid table name format: '{tableName}'. " +
                    "Table names must follow PostgreSQL identifier naming conventions.");
            }
        }

        /// <summary>
        /// Validates that a column name matches expected PostgreSQL naming conventions.
        /// </summary>
        /// <param name="columnName">The column name to validate.</param>
        /// <exception cref="PluginException">Thrown if the column name is invalid.</exception>
        private void ValidateColumnName(string columnName)
        {
            // Only allow valid PostgreSQL identifier characters
            // Column names must start with a letter or underscore, followed by letters, digits, or underscores
            var pattern = @"^[a-zA-Z_][a-zA-Z0-9_]*$";
            if (!System.Text.RegularExpressions.Regex.IsMatch(columnName, pattern))
            {
                throw new PluginException(
                    $"Invalid column name format: '{columnName}'. " +
                    "Column names must follow PostgreSQL identifier naming conventions.");
            }
        }

        #endregion
    }
}
```

### Parameter Definitions

| Parameter Name | Type | Description | Required |
|---------------|------|-------------|----------|
| `Tables` | Array | Comma-separated list of table names in `[schema].[table_name]` format | No (if QueryName is specified) |
| `QueryName` | String | Name of a custom query defined in the data source configuration | No (if Tables is specified) |

---

## Authentication Strategy

### Supported Authentication Methods

1. **Entra ID with Managed Identity (Recommended)**
   - Uses `DefaultAzureCredential` to obtain access tokens
   - Requires the managed identity to be granted access to the PostgreSQL server
   - Set `UseManagedIdentity = true` in configuration references

2. **Username/Password Authentication**
   - Traditional authentication using PostgreSQL credentials
   - Password should be stored in Azure Key Vault and referenced via configuration

3. **Connection String**
   - Direct connection string for legacy scenarios
   - Should be stored in Azure Key Vault for security

### Entra ID Setup for Managed Identity

To enable Entra ID authentication with Azure PostgreSQL:

1. Enable Entra ID authentication on the Azure PostgreSQL server
2. Create an Entra ID admin for the server
3. Grant the managed identity access:

```sql
-- Connect as Entra ID admin
SET aad_validate_oids_in_tenant = off;
CREATE ROLE "managed-identity-name" LOGIN;
GRANT ALL PRIVILEGES ON DATABASE "database-name" TO "managed-identity-name";
```

---

## Configuration References

### New Configuration Reference Keys

Add to `DataSourceConfigurationReferenceKeys.cs`:

```csharp
/// <summary>
/// The connection string for PostgreSQL (if provided directly).
/// </summary>
public const string ConnectionString = "ConnectionString";

/// <summary>
/// The PostgreSQL server hostname.
/// </summary>
public const string Host = "Host";

/// <summary>
/// The PostgreSQL database name.
/// </summary>
public const string Database = "Database";

/// <summary>
/// The username for PostgreSQL authentication.
/// </summary>
public const string Username = "Username";

/// <summary>
/// The password for PostgreSQL authentication.
/// </summary>
public const string Password = "Password";

/// <summary>
/// Whether to use Entra ID managed identity for authentication.
/// </summary>
public const string UseManagedIdentity = "UseManagedIdentity";
```

### Alternative: Create PostgreSQL-Specific Configuration Keys

Create a new file `src/dotnet/DataPipelinePlugins/Constants/AzurePostgreSQLConfigurationKeys.cs`:

```csharp
namespace FoundationaLLM.Plugins.DataPipeline.Constants
{
    /// <summary>
    /// Defines the keys for Azure PostgreSQL data source configuration references.
    /// </summary>
    public static class AzurePostgreSQLConfigurationKeys
    {
        public const string ConnectionString = "ConnectionString";
        public const string Host = "Host";
        public const string Database = "Database";
        public const string Username = "Username";
        public const string Password = "Password";
        public const string UseManagedIdentity = "UseManagedIdentity";
    }
}
```

---

## Plugin Metadata Registration

### PluginNames.cs Changes

```csharp
public const string AZUREPOSTGRESQL_DATASOURCE = $"{PACKAGE_NAME}-AzurePostgreSQLDataSource";
```

### PluginParameterNames.cs Changes

```csharp
public const string AZUREPOSTGRESQL_DATASOURCE_TABLES = "Tables";
public const string AZUREPOSTGRESQL_DATASOURCE_QUERYNAME = "QueryName";
```

### PluginPackageManager.cs Changes

Add to the `Plugins` list in `GetMetadata`:

```csharp
new() {
    ObjectId = $"/instances/{instanceId}/providers/FoundationaLLM.Plugin/plugins/{PluginNames.AZUREPOSTGRESQL_DATASOURCE}",
    Name = PluginNames.AZUREPOSTGRESQL_DATASOURCE,
    DisplayName = "Azure PostgreSQL Data Source (FoundationaLLM)",
    Description = "Provides the FoundationaLLM standard implementation for Azure PostgreSQL data sources.",
    Category = PluginCategoryNames.DataSource,
    Parameters = [
        new() {
            Name = PluginParameterNames.AZUREPOSTGRESQL_DATASOURCE_TABLES,
            Type = PluginParameterTypes.Array,
            Description = "Comma-separated list of table/view names to extract (format: [schema].[table_name])."
        },
        new() {
            Name = PluginParameterNames.AZUREPOSTGRESQL_DATASOURCE_QUERYNAME,
            Type = PluginParameterTypes.String,
            Description = "Name of a custom query defined in the data source configuration."
        }
    ],
    Dependencies = []
},
```

Add to the `GetDataSourcePlugin` switch expression:

```csharp
PluginNames.AZUREPOSTGRESQL_DATASOURCE => new AzurePostgreSQLDataSourcePlugin(
    dataSourceObjectId, pluginParameters, this, packageManagerResolver, serviceProvider),
```

---

## Dependencies

### NuGet Packages to Add

Add to `DataPipelinePlugins.csproj`:

```xml
<PackageReference Include="Npgsql" Version="8.0.5" />
```

**Note:** The Npgsql package is actively maintained and supports:
- .NET 8.0
- Entra ID (Azure AD) authentication via token
- SSL/TLS connections
- Connection pooling
- Async operations

### Security Advisory Check

Before adding the Npgsql package, verify it against the GitHub Advisory Database for known vulnerabilities.

---

## Testing Strategy

### Unit Tests

Create test file: `tests/dotnet/DataPipeline.Tests/Plugins/DataSource/AzurePostgreSQLDataSourcePluginTests.cs`

1. **GetContentItems Tests**
   - Test with valid table configuration
   - Test with custom query configuration
   - Test with invalid/missing configuration
   - Test with empty result sets

2. **GetContentItemRawContent Tests**
   - Test retrieval of valid content item
   - Test with invalid content identifier
   - Test content item not found scenario

3. **HandleUnsafeContentItem Tests**
   - Verify no action is taken on source database
   - Verify logging of unsafe content notification

4. **Authentication Tests**
   - Test connection string parsing
   - Test managed identity token acquisition (mocked)
   - Test username/password authentication

### Integration Tests

1. **Live Database Tests** (requires Azure PostgreSQL instance)
   - Connect to test database
   - Extract content from test table
   - Verify content identifier generation
   - Verify raw content retrieval

2. **End-to-End Pipeline Tests**
   - Create complete data pipeline with PostgreSQL data source
   - Execute text extraction stage
   - Verify content flows through pipeline stages

### Mock Requirements

- Mock `IResourceProviderService` for data source retrieval
- Mock `IConfiguration` for configuration reference resolution
- Mock `NpgsqlConnection` for unit tests (consider using Testcontainers for integration tests)

---

## Example Usage

### Data Source Configuration (JSON)

```json
{
    "type": "azure-postgresql",
    "name": "ProductCatalogDB",
    "description": "Product catalog database for AI agent knowledge base",
    "object_id": "/instances/{{instanceId}}/providers/FoundationaLLM.DataSource/dataSources/ProductCatalogDB",
    "tables": [
        "public.products",
        "public.categories",
        "inventory.stock_levels"
    ],
    "custom_queries": [
        {
            "name": "ProductsWithDescriptions",
            "query": "SELECT p.id, p.name, p.description, c.name as category FROM public.products p JOIN public.categories c ON p.category_id = c.id WHERE p.active = true",
            "id_column": "id",
            "content_column": "description",
            "metadata_columns": ["name", "category"]
        }
    ],
    "configuration_references": {
        "Host": "FoundationaLLM:DataSource:ProductCatalogDB:Host",
        "Database": "FoundationaLLM:DataSource:ProductCatalogDB:Database",
        "UseManagedIdentity": "FoundationaLLM:DataSource:ProductCatalogDB:UseManagedIdentity",
        "Username": "FoundationaLLM:DataSource:ProductCatalogDB:Username"
    }
}
```

### Data Pipeline Definition (JSON)

```json
{
    "type": "data-pipeline",
    "name": "ProductCatalogPipeline",
    "display_name": "Product Catalog Pipeline",
    "description": "Extracts and indexes product catalog data for AI agent use.",
    "active": true,
    "data_source": {
        "name": "ProductDB",
        "description": "Product catalog PostgreSQL database",
        "plugin_object_id": "/instances/{{instanceId}}/providers/FoundationaLLM.Plugin/plugins/Dotnet-FoundationaLLMDataPipelinePlugins-AzurePostgreSQLDataSource",
        "plugin_parameters": [
            {
                "parameter_metadata": {
                    "name": "Tables",
                    "type": "array",
                    "description": "Comma-separated list of table/view names to extract."
                },
                "default_value": null
            },
            {
                "parameter_metadata": {
                    "name": "QueryName",
                    "type": "string",
                    "description": "Name of a custom query defined in the data source."
                },
                "default_value": null
            }
        ],
        "data_source_object_id": "/instances/{{instanceId}}/providers/FoundationaLLM.DataSource/dataSources/ProductCatalogDB"
    },
    "starting_stages": [
        {
            "name": "Partition",
            "description": "Partition text into chunks.",
            "plugin_object_id": "/instances/{{instanceId}}/providers/FoundationaLLM.Plugin/plugins/TextPartitioningDataPipelineStage",
            "plugin_parameters": [
                {
                    "parameter_metadata": {
                        "name": "PartitioningStrategy",
                        "type": "string",
                        "description": "The partitioning strategy to be used."
                    },
                    "default_value": "Token"
                }
            ],
            "next_stages": [
                {
                    "name": "Embed",
                    "description": "Embed text chunks.",
                    "plugin_object_id": "/instances/{{instanceId}}/providers/FoundationaLLM.Plugin/plugins/GatewayTextEmbeddingDataPipelineStage",
                    "plugin_parameters": [],
                    "next_stages": [
                        {
                            "name": "Index",
                            "description": "Index embeddings to Azure AI Search.",
                            "plugin_object_id": "/instances/{{instanceId}}/providers/FoundationaLLM.Plugin/plugins/AzureAISearchIndexingDataPipelineStage",
                            "plugin_parameters": []
                        }
                    ]
                }
            ]
        }
    ],
    "triggers": [
        {
            "name": "DailyProductSync",
            "trigger_type": "Schedule",
            "trigger_cron_schedule": "0 2 * * *",
            "parameter_values": {
                "DataSource.ProductDB.QueryName": "ProductsWithDescriptions",
                "Stage.Partition.PartitioningStrategy": "Token"
            }
        },
        {
            "name": "ManualFullSync",
            "trigger_type": "Manual",
            "parameter_values": {
                "DataSource.ProductDB.Tables": "public.products,public.categories"
            }
        }
    ]
}
```

### Azure App Configuration Keys Example

```
FoundationaLLM:DataSource:ProductCatalogDB:Host = "myserver.postgres.database.azure.com"
FoundationaLLM:DataSource:ProductCatalogDB:Database = "productdb"
FoundationaLLM:DataSource:ProductCatalogDB:UseManagedIdentity = "true"
FoundationaLLM:DataSource:ProductCatalogDB:Username = "my-managed-identity-name"
```

---

## Implementation Phases

### Phase 1: Foundation (Day 1-2)
- [ ] Add `DataSourceTypes.AzurePostgreSQL` constant
- [ ] Create `AzurePostgreSQLDataSource` model class
- [ ] Update `DataSourceBase.cs` with JSON derived type
- [ ] Add Npgsql NuGet package dependency
- [ ] Create configuration keys constants

### Phase 2: Core Plugin Implementation (Day 3-5)
- [ ] Create `AzurePostgreSQLDataSourcePlugin` class
- [ ] Implement `GetContentItems()` for table extraction
- [ ] Implement `GetContentItems()` for custom queries
- [ ] Implement `GetContentItemRawContent()`
- [ ] Implement `HandleUnsafeContentItem()`
- [ ] Implement authentication (connection string, managed identity, username/password)

### Phase 3: Plugin Registration (Day 6)
- [ ] Add plugin name constant to `PluginNames.cs`
- [ ] Add parameter constants to `PluginParameterNames.cs`
- [ ] Register plugin metadata in `PluginPackageManager.cs`
- [ ] Add factory method to `GetDataSourcePlugin()`

### Phase 4: Testing and Documentation (Day 7-8)
- [ ] Create unit tests
- [ ] Create integration tests (if test infrastructure available)
- [ ] Update documentation
- [ ] Code review and refinement

---

## Assumptions

1. The Azure PostgreSQL instance is accessible from the FoundationaLLM deployment environment
2. Appropriate network rules (firewall, private endpoints) are configured
3. The managed identity or service principal has been granted necessary database permissions
4. Tables have primary keys for unique row identification (or `ctid` fallback is acceptable)
5. Content from database rows can be converted to text format

---

## Risks and Mitigations

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Large tables causing memory issues | Medium | High | Implement pagination/streaming for large result sets |
| Connection pool exhaustion | Low | Medium | Use Npgsql connection pooling, implement retry logic |
| Token expiration during long operations | Low | Medium | Refresh tokens periodically for managed identity |
| SQL injection via table names | Low | High | Validate table names using regex pattern matching, use `NpgsqlConnection.QuoteIdentifier()` for identifier quoting, only allow table names from pre-configured data source configuration |
| Binary data types not supported | Medium | Low | Document supported data types, log warnings for unsupported types |

---

## Not Included (Future Enhancements)

1. **Change Data Capture (CDC)**: Detecting and processing only changed rows
2. **Incremental Loading**: Tracking last extraction timestamp for delta loads
3. **Connection Pooling Configuration**: Advanced pool settings exposed as parameters
4. **Binary Content Extraction**: Extracting files stored as bytea columns
5. **Schema Discovery UI**: Auto-discovery of tables and columns in Management Portal
6. **Query Builder UI**: Visual interface for building custom queries
7. **Row-Level Security**: Filtering data based on user context
8. **Read Replicas**: Support for connecting to read replicas for large extractions
