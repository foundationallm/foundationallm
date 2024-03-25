using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class InternalContextCompletionRequestTests
    {
        [Fact]
        public void InternalContextCompletionRequest_Agent_Property_Test()
        {
            // Arrange
            var request = new LLMCompletionRequest() 
                { Agent = new InternalContextAgent() { Name = "Test_agent", ObjectId = "Test_objectid", Type = AgentTypes.InternalContext} };

            var agent = new InternalContextAgent() { Name = "Test_agent", ObjectId = "Test_objectid", Type = AgentTypes.InternalContext };

            // Act
            request.Agent = agent;

            // Assert
            Assert.Equal(agent, request.Agent);
        }
    }
}
