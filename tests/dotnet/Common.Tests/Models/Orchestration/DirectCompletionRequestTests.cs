using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class DirectCompletionRequestTests
    {
        [Fact]
        public void DirectCompletionRequest_UserPrompt_Property_Test()
        {
            // Arrange
            var request = new DirectCompletionRequest();
            var userPrompt = "Test user prompt";

            // Act
            request.UserPrompt = userPrompt;

            // Assert
            Assert.Equal(userPrompt, request.UserPrompt);
        }
    }
}
