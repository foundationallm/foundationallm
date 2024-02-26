using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class OrchestrationRequestTests
    {
        [Fact]
        public void OrchestrationRequest_Properties_Test()
        {
            // Arrange
            string expectedSessionId = "12345";
            string expectedUserPrompt = "Test user prompt";

            // Act
            var orchestrationRequest = new OrchestrationRequest
            {
                SessionId = expectedSessionId,
                UserPrompt = expectedUserPrompt
            };

            // Assert
            Assert.Equal(expectedSessionId, orchestrationRequest.SessionId);
            Assert.Equal(expectedUserPrompt, orchestrationRequest.UserPrompt);
        }
    }
}
