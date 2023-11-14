using FoundationaLLM.AgentFactory.Core.Models.Messages;

namespace FoundationaLLM.AgentFactory.Tests.Models.Messages
{
    public class PromptHubRequestTests
    {
        [Fact]
        public void PromptHubRequest_WhenInitialized_ShouldSetAgentNameProperty()
        {
            // Arrange
            string agentName = "TestAgent";

            // Act
            var request = new PromptHubRequest { AgentName = agentName };

            // Assert
            Assert.Equal(agentName, request.AgentName);
        }
    }
}
