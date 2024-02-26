using FoundationaLLM.Common.Models.Agents;
using FoundationaLLM.Common.Models.Configuration.Users;

namespace FoundationaLLM.Common.Tests.Models.Configuration.Users
{
    public class UserProfileTests
    {
        [Fact]
        public void UserProfile_Properties_SetCorrectly()
        {
            // Arrange
            string expectedUPN = "testuser@example.com";
            var expectedPrivateAgents = new List<AgentHint>
        {
            new AgentHint { Name = "Agent1", Private = true },
            new AgentHint { Name = "Agent2", Private = true }
        };

            // Act
            var userProfile = new UserProfile(expectedUPN, expectedPrivateAgents);

            // Assert
            Assert.Equal(expectedUPN, userProfile.UPN);
            Assert.Equal(expectedUPN, userProfile.Id);
            Assert.Equal(nameof(UserProfile), userProfile.Type);
            Assert.Equal(expectedPrivateAgents, userProfile.PrivateAgents);
        }
    }
}
