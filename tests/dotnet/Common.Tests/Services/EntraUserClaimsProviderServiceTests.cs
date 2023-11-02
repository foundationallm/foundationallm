using FoundationaLLM.Common.Services;
using System.Security.Claims;

namespace FoundationaLLM.Common.Tests.Services
{
    public class EntraUserClaimsProviderServiceTests
    {
        private EntraUserClaimsProviderService _userClaimsProviderService;
        public EntraUserClaimsProviderServiceTests()
        {
            _userClaimsProviderService = new EntraUserClaimsProviderService();
        }

        [Fact]
        public void GetUserIdentity_ReturnsNull_WhenUserPrincipalIsNull()
        {
            // Arrange
            ClaimsPrincipal? userPrincipal = null;

            // Act
            var result = _userClaimsProviderService.GetUserIdentity(userPrincipal);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetUserIdentity_ReturnsUnifiedUserIdentity_WhenUserPrincipalIsNotNull()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim("name", "Test_1"),
                new Claim("preferred_username", "test1"),
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuthentication");
            ClaimsPrincipal userPrincipal = new ClaimsPrincipal(identity);

            // Act
            var result = _userClaimsProviderService.GetUserIdentity(userPrincipal);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test_1", result?.Name);
            Assert.Equal("test1", result?.Username);
            Assert.Equal("test1", result?.UPN);
        }
    }
}
