using FoundationaLLM.Common.Models.Metadata;

namespace FoundationaLLM.Common.Tests.Models.Metadata
{
    public class EmbeddingModelTests
    {
        [Fact]
        public void EmbeddingModel_Properties_Initialized_Correctly()
        {
            // Arrange
            var embeddingModel = new EmbeddingModel();

            // Assert
            Assert.Null(embeddingModel.Type);
            Assert.Null(embeddingModel.Provider);
            Assert.Null(embeddingModel.Deployment);
            Assert.Null(embeddingModel.Model);
            Assert.Equal(1000, embeddingModel.ChunkSize);
            Assert.Null(embeddingModel.ApiEndpoint);
            Assert.Null(embeddingModel.ApiKey);
            Assert.Null(embeddingModel.ApiVersion);
        }
    }
}
