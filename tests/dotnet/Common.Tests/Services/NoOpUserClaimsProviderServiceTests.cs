using FoundationaLLM.Common.Services;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace FoundationaLLM.Common.Tests.Services
{
    public class NoOpUserClaimsProviderServiceTests
    {
        [Fact]
        public void GetUserIdentity_ShouldReturnNull()
        {
            // Arrange
            var userClaimsProviderService = new NoOpUserClaimsProviderService();
            ClaimsPrincipal userPrincipal = new ClaimsPrincipal();

            // Act
            var result = userClaimsProviderService.GetUserIdentity(userPrincipal);

            // Assert
            Assert.Null(result);
        }
    }
}
