using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class LLMCompletionRequestTests
    {
        [Fact]
        public void LLMCompletionRequest_Properties_Test()
        {
            // Arrange
            var request = new LLMCompletionRequest
            {
                SessionId = "Test sessionid",
                UserPrompt = "Test prompt"
            };

            // Assert
            Assert.Equal("Test sessionid", request.SessionId);
            Assert.Equal("Test prompt", request.UserPrompt);
        }
    }
}
