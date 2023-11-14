using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration.Metadata
{
    public class AgentTests
    {
        private readonly string _promptSufix = "Sufix_1";
        private readonly string _promptPrefix = "Prefix_1";

        private readonly Agent _agent;

        public AgentTests()
        {
            _agent = new Agent { PromptPrefix = _promptPrefix, PromptSuffix = _promptSufix };
        }

        [Fact]
        public void Agent_WhenInitialized_ShouldSetProperties()
        {
            // Assert
            Assert.Equal(_promptSufix, _agent.PromptSuffix);
            Assert.Equal(_promptPrefix, _agent.PromptPrefix);
        }

        [Fact]
        public void Agent_JsonSerializationTest()
        {
            // Act
            var serializedJson = JsonConvert.SerializeObject(_agent);
            var deserializedAgent = JsonConvert.DeserializeObject<Agent>(serializedJson);

            // Assert
            Assert.Equal(_agent.PromptPrefix, deserializedAgent?.PromptPrefix);
            Assert.Equal(_agent.PromptSuffix, deserializedAgent?.PromptSuffix);
        }
    }
}
