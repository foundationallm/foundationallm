using FoundationaLLM.Common.Models.Hubs;

namespace FoundationaLLM.Common.Tests.Models.Hubs
{
    public class AgentHubResponseTests
    {
        [Fact]
        public void AgentMetadata_Contains_Properties()
        {
            // Arrange
            var agentMetadata = new AgentMetadata();

            // Assert
            Assert.Null(agentMetadata.Orchestrator);
            Assert.Null(agentMetadata.AllowedDataSourceNames);
            Assert.Null(agentMetadata.LanguageModel);
            Assert.Null(agentMetadata.EmbeddingModel);
            Assert.Null(agentMetadata.MaxMessageHistorySize);
            Assert.Null(agentMetadata.PromptContainer);
        }
    }

    public class AgentMetadataTests
    {
        [Fact]
        public void AgentMetadata_Contains_Properties()
        {
            // Arrange
            var agentMetadata = new AgentMetadata();

            // Assert
            Assert.Null(agentMetadata.Orchestrator);
            Assert.Null(agentMetadata.AllowedDataSourceNames);
            Assert.Null(agentMetadata.LanguageModel);
            Assert.Null(agentMetadata.EmbeddingModel);
            Assert.Null(agentMetadata.MaxMessageHistorySize);
            Assert.Null(agentMetadata.PromptContainer);
        }
    }
}
