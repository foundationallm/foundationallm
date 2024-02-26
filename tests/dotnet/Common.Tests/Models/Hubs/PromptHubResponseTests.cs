using FoundationaLLM.Common.Models.Hubs;

namespace FoundationaLLM.Common.Tests.Models.Hubs
{
    public class PromptHubResponseTests
    {
        [Fact]
        public void PromptHubResponse_Contains_PromptMetadata()
        {
            // Arrange & Act
            var promptHubResponse = new PromptHubResponse();

            // Assert
            Assert.Null(promptHubResponse.Prompt);
        }
    }

    public class PromptMetadataTests
    {
        [Fact]
        public void PromptMetadata_Initialized_Correctly()
        {
            // Arrange & Act
            var promptMetadata = new PromptMetadata();

            // Assert
            Assert.Null(promptMetadata.Name);
            Assert.Null(promptMetadata.PromptPrefix);
            Assert.Null(promptMetadata.PromptSuffix);
        }
    }
}
