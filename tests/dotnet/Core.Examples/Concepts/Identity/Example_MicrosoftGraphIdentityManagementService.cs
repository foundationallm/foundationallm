using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Core.Examples.Setup;
using FoundationaLLM.Tests;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Concepts.Identity
{
    /// <summary>
    /// Example class for testing the Microsoft Graph Identity Management Service.
    /// </summary>
    public class Example_MicrosoftGraphIdentityManagementService : TestBase, IClassFixture<TestFixture>
    {
        public Example_MicrosoftGraphIdentityManagementService(ITestOutputHelper output, TestFixture fixture)
            : base(1, output, fixture, new DependencyInjectionContainerInitializer())
        {
        }

        [Fact]
        public async Task MicrosoftGraphIdentityManagementService_RetrieveGroupsAndUsers()
        {
            // Wait for all required initialization tasks to complete.
            await Task.WhenAll([
                StartEventsWorkers()
            ]);

            var identityManagementService = GetService<FoundationaLLM.Common.Interfaces.IIdentityManagementService>();
            
            WriteLine("============ FoundationaLLM Microsoft Graph Identity Management Service Tests ============");
            
            var securityPrincipals = await identityManagementService.GetObjectsByIds(new ObjectQueryParameters
            {
                Ids = [
                    ServiceContext.ServiceIdentity!.UserId!
                ]
            });
            Assert.NotNull(securityPrincipals);
            Assert.True(securityPrincipals.Count > 0);

            var users = await identityManagementService.GetUsers(new ObjectQueryParameters
            {
                Name = "cip",
                Ids = []
            });
            Assert.NotNull(users);
            Assert.True(users.TotalItems > 0);

            var groups = await identityManagementService.GetUserGroups(new ObjectQueryParameters
            {
                Name = "FLLM",
                Ids = []
            });
            Assert.NotNull(groups);
            Assert.True(groups.TotalItems > 0);

            var servicePrincipals = await identityManagementService.GetServicePrincipals(new ObjectQueryParameters
            {
                Name = "mi-",
                Ids = []
            });
            Assert.NotNull(servicePrincipals);
            Assert.True(servicePrincipals.TotalItems > 0);
        }
    }
}
