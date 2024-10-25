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
            var agentType = AgentTypes.KnowledgeManagement;

            // Assert
            Assert.Equal("knowledge-management", agentType);
        }

        [Fact]
        public void AnalyticAgentType_IsCorrect()
        {
            // Arrange & Act
            var agentType = AgentTypes.Analytic;

            // Assert
            Assert.Equal("analytic", agentType);
        }
    }
}
