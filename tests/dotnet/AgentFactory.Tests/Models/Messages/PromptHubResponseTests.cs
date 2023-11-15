using Azure;
using FoundationaLLM.AgentFactory.Core.Models.Messages;

namespace FoundationaLLM.AgentFactory.Tests.Models.Messages
{
    public class PromptHubResponseTests
    {
        [Fact]
        public void PromptHubResponse_WhenInitialized_ShouldSetPromptsProperty()
        {
            // Arrange
            var prompt = new PromptMetadata { Name = "Prompt1", PromptPrefix = "Prefix_1", PromptSuffix = "Suffix_1" };

            // Act
            var response = new PromptHubResponse {  Prompt = prompt };

            // Assert
            Assert.Equal(prompt, response.Prompt);
        }
    }

    public class PromptMetadataTests
    {
        [Fact]
        public void PromptMetadata_WhenInitialized_ShouldSetProperties()
        {
            // Arrange
            string name = "TestName";
            string promptPrefix = "Prefix_1";
            string promptSufix = "Sufix_1";


            // Act
            var metadata = new PromptMetadata { Name = name, PromptPrefix = promptPrefix, PromptSuffix = promptSufix };

            // Assert
            Assert.Equal(name, metadata.Name);
            Assert.Equal(promptSufix, metadata.PromptSuffix);
            Assert.Equal(promptPrefix, metadata.PromptPrefix);
        }
    }
}
