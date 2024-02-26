using FoundationaLLM.Common.Models.Hubs;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Common.Tests.Models.Hubs
{
    public class AgentHubRequestTests
    {
        [Fact]
        public void AgentHubRequest_InheritsFrom_OrchestrationRequest()
        {
            // Arrange
            var agentHubRequest = new AgentHubRequest() { UserPrompt = "Test_userprompt"};

            // Act
            bool inherits = agentHubRequest is OrchestrationRequest;


            // Assert
            Assert.True(inherits);
        }
    }
}
