using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Tests.Models.Orchestration.Metadata
{
    public class AgentTests
    {
        private readonly string _promptTemplate = "TestPromptTemplate";
        private readonly Agent _agent;

        public AgentTests()
        {
            _agent = new Agent { PromptTemplate = _promptTemplate };
        }

        [Fact]
        public void Agent_WhenInitialized_ShouldSetProperties()
        {
            // Assert
            Assert.Equal(_promptTemplate, _agent.PromptTemplate);
        }

        [Fact]
        public void Agent_JsonSerializationTest()
        {
            // Act
            var serializedJson = JsonConvert.SerializeObject(_agent);
            var deserializedAgent = JsonConvert.DeserializeObject<Agent>(serializedJson);

            // Assert
            Assert.Equal(_agent.PromptTemplate, deserializedAgent?.PromptTemplate);
        }
    }
}
