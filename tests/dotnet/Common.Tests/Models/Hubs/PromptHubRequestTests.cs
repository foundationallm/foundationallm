using FoundationaLLM.Common.Models.Hubs;

namespace FoundationaLLM.Common.Tests.Models.Hubs
{
    public class PromptHubRequestTests
    {
        [Fact]
        public void PromptHubRequest_Contains_Properties()
        {
            // Act
            var promptHubRequest = new PromptHubRequest();

            // Assert
            Assert.Null(promptHubRequest.SessionId);
            Assert.Null(promptHubRequest.PromptContainer);
            Assert.Equal("default", promptHubRequest.PromptName);
        }
    }
}
