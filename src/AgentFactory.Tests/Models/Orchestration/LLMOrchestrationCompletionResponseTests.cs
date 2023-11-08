using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration
{
    public class LLMOrchestrationCompletionResponseTests
    {
        private readonly string _completion = "TestCompletion";
        private readonly string _userPrompt = "TestUserPrompt";
        private readonly int _promptTokens = 5;
        private readonly int _completionTokens = 10;
        private readonly int _totalTokens = 15;
        private readonly float _totalCost = 2.5f;
        private readonly LLMOrchestrationCompletionResponse _completionResponse;

        public LLMOrchestrationCompletionResponseTests()
        {
            _completionResponse = new LLMOrchestrationCompletionResponse
            {
                Completion = _completion,
                UserPrompt = _userPrompt,
                PromptTokens = _promptTokens,
                CompletionTokens = _completionTokens,
                TotalTokens = _totalTokens,
                TotalCost = _totalCost
            };
        }

        [Fact]
        public void LLMOrchestrationCompletionResponse_WhenInitialized_ShouldSetProperties()
        {
            // Assert
            Assert.Equal(_completion, _completionResponse.Completion);
            Assert.Equal(_userPrompt, _completionResponse.UserPrompt);
            Assert.Equal(_promptTokens, _completionResponse.PromptTokens);
            Assert.Equal(_completionTokens, _completionResponse.CompletionTokens);
            Assert.Equal(_totalTokens, _completionResponse.TotalTokens);
            Assert.Equal(_totalCost, _completionResponse.TotalCost);
        }

        [Fact]
        public void LLMOrchestrationCompletionResponse_JsonSerializationTest()
        {
            // Act
            var serializedJson = JsonConvert.SerializeObject(_completionResponse);
            var deserializedCompletionResponse = JsonConvert.DeserializeObject<LLMOrchestrationCompletionResponse>(serializedJson);

            // Assert
            Assert.Equal(_completion, deserializedCompletionResponse?.Completion);
            Assert.Equal(_userPrompt, deserializedCompletionResponse?.UserPrompt);
            Assert.Equal(_promptTokens, deserializedCompletionResponse?.PromptTokens);
            Assert.Equal(_completionTokens, deserializedCompletionResponse?.CompletionTokens);
            Assert.Equal(_totalTokens, deserializedCompletionResponse?.TotalTokens);
            Assert.Equal(_totalCost, deserializedCompletionResponse?.TotalCost);
        }
    }
}
