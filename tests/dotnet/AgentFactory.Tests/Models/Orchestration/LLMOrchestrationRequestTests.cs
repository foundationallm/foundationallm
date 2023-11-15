using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration
{
    public class LLMOrchestrationRequestTests
    {
        private readonly string _userPrompt = "TestUserPrompt";
        private readonly LLMOrchestrationRequest _orchestrationRequest;

        public LLMOrchestrationRequestTests()
        {
            _orchestrationRequest = new LLMOrchestrationRequest { UserPrompt = _userPrompt };
        }

        [Fact]
        public void LLMOrchestrationRequest_WhenInitialized_ShouldSetProperties()
        {
            // Assert
            Assert.Equal(_userPrompt, _orchestrationRequest.UserPrompt);
        }

        [Fact]
        public void LLMOrchestrationRequest_JsonSerializationTest()
        {
            // Act
            var serializedJson = JsonConvert.SerializeObject(_orchestrationRequest);
            var deserializedOrchestrationRequest = JsonConvert.DeserializeObject<LLMOrchestrationRequest>(serializedJson);

            // Assert
            Assert.Equal(_orchestrationRequest.UserPrompt, deserializedOrchestrationRequest?.UserPrompt);
        }
    }
}
