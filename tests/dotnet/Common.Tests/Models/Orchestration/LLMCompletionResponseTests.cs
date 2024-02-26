using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Common.Tests.Models.Orchestration
{
    public class LLMCompletionResponseTests
    {
        [Fact]
        public void LLMCompletionResponse_Properties_Test()
        {
            // Arrange
            var response = new LLMCompletionResponse
            {
                Completion = "Completed response",
                UserPrompt = "User prompt",
                FullPrompt = "Full prompt",
                PromptTemplate = "Prompt template",
                AgentName = "Agent name",
                PromptTokens = 10,
                CompletionTokens = 20,
                TotalTokens = 30,
                TotalCost = 15.5f
            };

            // Act & Assert
            Assert.Equal("Completed response", response.Completion);
            Assert.Equal("User prompt", response.UserPrompt);
            Assert.Equal("Full prompt", response.FullPrompt);
            Assert.Equal("Prompt template", response.PromptTemplate);
            Assert.Equal("Agent name", response.AgentName);
            Assert.Equal(10, response.PromptTokens);
            Assert.Equal(20, response.CompletionTokens);
            Assert.Equal(30, response.TotalTokens);
            Assert.Equal(15.5f, response.TotalCost);
        }
    }
}
