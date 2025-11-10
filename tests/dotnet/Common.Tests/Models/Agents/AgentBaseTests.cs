using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;

namespace FoundationaLLM.Common.Tests.Models.Agents
{
    public class AgentBaseTests
    {
        private AgentBase _agentBase = new AgentBase { Type = AgentTypes.GenericAgent, Name = "Test_agent", ObjectId = "Test_objectid" };

        [Fact]
        public void AgentType_KnowledgeManagement_ReturnsCorrectType()
        {
            // Assert
            Assert.Equal(typeof(GenericAgent), _agentBase.AgentType);
        }

        [Fact]
        public void AgentType_UnsupportedType_ThrowsException()
        {
            // Arrange
            _agentBase.Type = "Test_Type";

            // Act & Assert
            Assert.Throws<ResourceProviderException>(() => _agentBase.AgentType);
        }

        [Fact]
        public void ConversationHistory_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var conversationHistory = new AgentConversationHistorySettings { Enabled = true, MaxHistory = 100 };
            _agentBase.ConversationHistorySettings = conversationHistory;

            // Assert
            Assert.Equal(conversationHistory, _agentBase.ConversationHistorySettings);
        }

        [Fact]
        public void Gatekeeper_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var gatekeeper = new AgentGatekeeperSettings { UseSystemSetting = false, Options = new string[] { "Option1", "Option2" } };
            _agentBase.GatekeeperSettings = gatekeeper;

            // Assert
            Assert.Equal(gatekeeper, _agentBase.GatekeeperSettings);
        }

    }
}
