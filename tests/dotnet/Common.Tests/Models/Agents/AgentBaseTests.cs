using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Agents;

namespace FoundationaLLM.Common.Tests.Models.Agents
{
    public class AgentBaseTests
    {
        private AgentBase _agentBase = new AgentBase { Type = AgentTypes.KnowledgeManagement, Name = "Test_agent", ObjectId = "Test_objectid" };

        [Fact]
        public void AgentType_KnowledgeManagement_ReturnsCorrectType()
        {
            // Assert
            Assert.Equal(typeof(KnowledgeManagementAgent), _agentBase.AgentType);
        }

        [Fact]
        public void AgentType_InternalContext_ReturnsCorrectType()
        {
            // Arrange
            _agentBase.Type = AgentTypes.InternalContext;

            // Assert
            Assert.Equal(typeof(KnowledgeManagementAgent), _agentBase.AgentType);
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
            var conversationHistory = new ConversationHistory { Enabled = true, MaxHistory = 100 };
            _agentBase.ConversationHistory = conversationHistory;

            // Assert
            Assert.Equal(conversationHistory, _agentBase.ConversationHistory);
        }

        [Fact]
        public void Gatekeeper_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var gatekeeper = new Gatekeeper { UseSystemSetting = false, Options = new string[] { "Option1", "Option2" } };
            _agentBase.Gatekeeper = gatekeeper;

            // Assert
            Assert.Equal(gatekeeper, _agentBase.Gatekeeper);
        }

    }
}
