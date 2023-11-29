using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;

namespace FoundationaLLM.AgentFactory.Tests.Models.Messages
{
    public class AgentHubResponseTests
    {
        [Fact]
        public void AgentHubResponse_WhenInitialized_ShouldSetAgentProperty()
        {
            // Arrange
            var agent = new AgentMetadata();

            // Act
            var response = new AgentHubResponse { Agent = agent };

            // Assert
            Assert.Equal(agent, response.Agent);
        }
    }

    public class AgentMetadataTests
    {
        [Fact]
        public void AgentMetadata_WhenInitialized_ShouldSetProperties()
        {
            // Arrange
            var languageModel = new LanguageModel();
            var allowedDataSourceNames = new List<string> { "TestDataSource" };

            // Act
            var metadata = new AgentMetadata
            {
                Name = "TestName",
                Type = "TestType",
                Description = "TestDescription",
                Orchestrator = "TestOrchestrator",
                AllowedDataSourceNames = allowedDataSourceNames,
                LanguageModel = languageModel
            };

            // Assert
            Assert.Equal("TestName", metadata.Name);
            Assert.Equal("TestType", metadata.Type);
            Assert.Equal("TestDescription", metadata.Description);
            Assert.Equal("TestOrchestrator", metadata.Orchestrator);
            Assert.Equal(allowedDataSourceNames, metadata.AllowedDataSourceNames);
            Assert.Equal(languageModel, metadata.LanguageModel);
        }
    }

    public class LanguageModelMetadataTests
    {
        [Fact]
        public void LanguageModelMetadata_WhenInitialized_ShouldSetProperties()
        {
            // Arrange
            var type = "TestType";
            var provider = "TestProvider";
            var temperature = 0.5f;
            var useChat = true;

            // Act
            var metadata = new LanguageModel
            {
                Type = type,
                Provider = provider,
                Temperature = temperature,
                UseChat = useChat
            };

            // Assert
            Assert.Equal(type, metadata.Type);
            Assert.Equal(provider, metadata.Provider);
            Assert.Equal(temperature, metadata.Temperature);
            Assert.Equal(useChat, metadata.UseChat);
        }
    }
}
