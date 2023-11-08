using FoundationaLLM.AgentFactory.Core.Models.Messages;

namespace FoundationaLLM.AgentFactory.Tests.Models.Messages
{
    public class PromptHubResponseTests
    {
        [Fact]
        public void PromptHubResponse_WhenInitialized_ShouldSetPromptsProperty()
        {
            // Arrange
            var prompts = new PromptMetadata[]
            {
            new PromptMetadata { Name = "Prompt1", Prompt = "This is Prompt 1" },
            new PromptMetadata { Name = "Prompt2", Prompt = "This is Prompt 2" }
            };

            // Act
            var response = new PromptHubResponse { Prompts = prompts };

            // Assert
            Assert.Equal(prompts, response.Prompts);
        }
    }

    public class PromptMetadataTests
    {
        [Fact]
        public void PromptMetadata_WhenInitialized_ShouldSetProperties()
        {
            // Arrange
            string name = "TestName";
            string prompt = "TestPrompt";

            // Act
            var metadata = new PromptMetadata { Name = name, Prompt = prompt };

            // Assert
            Assert.Equal(name, metadata.Name);
            Assert.Equal(prompt, metadata.Prompt);
        }
    }
}
