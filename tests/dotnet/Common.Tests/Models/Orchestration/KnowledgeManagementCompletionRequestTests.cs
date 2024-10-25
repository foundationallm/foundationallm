using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class KnowledgeManagementCompletionRequestTests
    {
        [Fact]
        public void KnowledgeManagementCompletionRequest_Agent_Property_Test()
        {
            // Arrange
            var request = new LLMCompletionRequest() 
                { 
                    OperationId = Guid.NewGuid().ToString(),
                    UserPrompt="", 
                    Agent = new KnowledgeManagementAgent() { Name = "Test_agent", ObjectId = "Test_objectid", Type = AgentTypes.KnowledgeManagement }
                };

            var agent = new KnowledgeManagementAgent() { Name = "Test_agent", ObjectId = "Test_objectid", Type = AgentTypes.KnowledgeManagement };

            // Act
            request.Agent = agent;

            // Assert
            Assert.Equal(agent, request.Agent);
        }
    }
}
