using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Metadata;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.DataSources;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class LegacyCompletionRequestTests
    {
        [Fact]
        public void LegacyCompletionRequest_Properties_Test()
        {
            // Arrange
            var request = new LegacyCompletionRequest();
            var agent = new LegacyAgentMetadata();
            var dataSourceMetadata = new List<DataSourceBase>();
            var languageModel = new LanguageModel();
            var embeddingModel = new EmbeddingModel();
            var messageHistory = new List<MessageHistoryItem>();

            // Act
            request.Agent = agent;
            request.DataSourceMetadata = dataSourceMetadata;
            request.LanguageModel = languageModel;
            request.EmbeddingModel = embeddingModel;
            request.MessageHistory = messageHistory;

            // Assert
            Assert.Equal(agent, request.Agent);
            Assert.Equal(dataSourceMetadata, request.DataSourceMetadata);
            Assert.Equal(languageModel, request.LanguageModel);
            Assert.Equal(embeddingModel, request.EmbeddingModel);
            Assert.Equal(messageHistory, request.MessageHistory);
        }
    }
}
