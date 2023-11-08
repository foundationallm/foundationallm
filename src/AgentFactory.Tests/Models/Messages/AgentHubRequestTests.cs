using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Tests.Models.Messages
{
    public class AgentHubRequestTests
    {
        [Fact]
        public void AgentHubRequest_WhenInitialized_ShouldInheritUserPromptProperty()
        {
            // Arrange
            string userPrompt = "Test User Prompt";

            // Act
            var request = new AgentHubRequest { UserPrompt = userPrompt };

            // Assert
            Assert.Equal(userPrompt, request.UserPrompt);
        }
    }
}
