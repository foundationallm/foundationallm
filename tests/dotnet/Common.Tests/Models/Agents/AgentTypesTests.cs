using FoundationaLLM.Common.Models.ResourceProviders.Agent;

namespace FoundationaLLM.Common.Tests.Models.Agents
{
    public class AgentTypesTests
    {
        [Fact]
        public void BasicAgentType_IsCorrect()
        {
            // Arrange & Act
            var agentType = AgentTypes.Basic;

            // Assert
            Assert.Equal("basic", agentType);
        }

        [Fact]
        public void KnowledgeManagementAgentType_IsCorrect()
        {
            // Arrange & Act
            var agentType = AgentTypes.GenericAgent;

            // Assert
            Assert.Equal("generic-agent", agentType);
        }
    }
}
