using FoundationaLLM.Common.Models.Agents;

namespace FoundationaLLM.Common.Tests.Models.Agents
{
    public class AgentHintTests
    {
        [Fact]
        public void AgentHint_Name_SetCorrectly()
        {
            // Arrange
            var name = "TestName";
            var agentHint = new AgentHint { Name = name };

            // Assert
            Assert.Equal(name, agentHint.Name);
        }

        [Fact]
        public void AgentHint_Private_SetCorrectly_True()
        {
            // Arrange
            var agentHint = new AgentHint { Private = true };

            // Assert
            Assert.True(agentHint.Private);
        }

        [Fact]
        public void AgentHint_Private_SetCorrectly_False()
        {
            // Arrange
            var agentHint = new AgentHint { Private = false };

            // Assert
            Assert.False(agentHint.Private);
        }
    }
}
