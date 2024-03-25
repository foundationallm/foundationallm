using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class KnowledgeManagementCompletionRequestTests
    {
        [Fact]
        public void KnowledgeManagementCompletionRequest_Agent_Property_Test()
        {
            // Arrange
            var request = new LLMCompletionRequest() 
                { Agent = new KnowledgeManagementAgent() { Name = "Test_agent", ObjectId = "Test_objectid", Type = AgentTypes.KnowledgeManagement } };

            var agent = new KnowledgeManagementAgent() { Name = "Test_agent", ObjectId = "Test_objectid", Type = AgentTypes.KnowledgeManagement };

            // Act
            request.Agent = agent;

            // Assert
            Assert.Equal(agent, request.Agent);
        }
    }
}
