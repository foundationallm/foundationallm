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
            
            // Act
            var userProfile = new UserProfile(expectedUPN);

            // Assert
            Assert.Equal(expectedUPN, userProfile.UPN);
            Assert.Equal(expectedUPN, userProfile.Id);
            Assert.Equal(nameof(UserProfile), userProfile.Type);
        }
    }
}
