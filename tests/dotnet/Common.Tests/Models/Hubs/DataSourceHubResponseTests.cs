using FoundationaLLM.Common.Models.Hubs;

namespace FoundationaLLM.Common.Tests.Models.Hubs
{
    public class DataSourceHubResponseTests
    {
        [Fact]
        public void DataSourceHubResponse_Contains_DataSources_Property()
        {
            // Arrange
            var dataSourceHubResponse = new DataSourceHubResponse();

            // Assert
            Assert.Null(dataSourceHubResponse.DataSources);
        }
    }

    public class DataSourceMetadataTests
    {
        [Fact]
        public void DataSourceMetadata_Contains_Properties()
        {
            // Arrange
            var dataSourceMetadata = new DataSourceMetadata();

            // Assert
            Assert.Null(dataSourceMetadata.Name);
            Assert.Null(dataSourceMetadata.Description);
            Assert.Null(dataSourceMetadata.UnderlyingImplementation);
            Assert.Null(dataSourceMetadata.FileType);
            Assert.Null(dataSourceMetadata.Authentication);
            Assert.Null(dataSourceMetadata.Container);
            Assert.Null(dataSourceMetadata.Files);
            Assert.Null(dataSourceMetadata.DataDescription);
            Assert.Null(dataSourceMetadata.Dialect);
            Assert.Null(dataSourceMetadata.IncludeTables);
            Assert.Null(dataSourceMetadata.ExcludeTables);
            Assert.Null(dataSourceMetadata.FewShotExampleCount);
            Assert.Null(dataSourceMetadata.RowLevelSecurityEnabled);
            Assert.Null(dataSourceMetadata.IndexName);
            Assert.Null(dataSourceMetadata.TopN);
            Assert.Null(dataSourceMetadata.EmbeddingFieldName);
            Assert.Null(dataSourceMetadata.TextFieldName);
            Assert.Null(dataSourceMetadata.Sources);
            Assert.Null(dataSourceMetadata.RetrieverMode);
            Assert.Null(dataSourceMetadata.Company);
        }
    }

    public class SQLDataSourceMetadataTests
    {
        [Fact]
        public void SQLDataSourceMetadata_Inherits_DataSourceMetadata()
        {
            // Arrange
            var sqlDataSourceMetadata = new SQLDataSourceMetadata();

            // Act
            bool inherits = sqlDataSourceMetadata is DataSourceMetadata;

            // Assert
            Assert.True(inherits);
        }
    }

    public class BlobStorageDataSourceMetadataTests
    {
        [Fact]
        public void BlobStorageDataSourceMetadata_Inherits_DataSourceMetadata()
        {
            // Arrange
            var blobStorageDataSourceMetadata = new BlobStorageDataSourceMetadata();

            // Act
            bool inherits = blobStorageDataSourceMetadata is DataSourceMetadata;

            // Assert
            Assert.True(inherits);
        }
    }
}
