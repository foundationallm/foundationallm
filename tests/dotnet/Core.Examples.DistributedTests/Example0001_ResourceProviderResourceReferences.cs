using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Core.Examples.DistributedTests.ResourceProviders;
using FoundationaLLM.Core.Examples.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.DistributedTests
{
    /// <summary>
    /// Runs load tests on resource provider resource references.
    /// </summary>
    public class Example0001_ResourceProviderResourceReferences : DistributedTestBase, IClassFixture<TestFixture>
    {
        public Example0001_ResourceProviderResourceReferences(ITestOutputHelper output, TestFixture fixture)
			: base(1, output, fixture)
        {
        }

        [Fact]
        public async Task RunAsync()
        {
            WriteLine("============ FoundationaLLM Resource Provider Load Test ============");

            var simulatedUsersCount = 20;

            // Get resource providers in all DI containers.
            var resourceProvidersHosts = ServiceProviders
                .Select(sp => new LoadTestResourceProviders(sp, Output))
                .ToList();

            // Initialize all resource providers.
            await Task.WhenAll(resourceProvidersHosts
                .Select(rps => rps.InitializeAll()));

            await Task.WhenAll(
                Enumerable.Range(1, resourceProvidersHosts.Count)
                .Select(i => SimulateServiceHostLoad(
                    i,
                    simulatedUsersCount,
                    resourceProvidersHosts[i],
                    ServiceProviders[i]))
                );
        }

        private async Task SimulateServiceHostLoad(
            int hostId,
            int simulatedUsersCount,
            LoadTestResourceProviders resourceProviders,
            IServiceProvider serviceProvider)
        {
            var instanceSettings = serviceProvider.GetRequiredService<IOptions<InstanceSettings>>().Value;
            var instanceId = instanceSettings.Id;
            var userIdentities = GetUserIdentities(hostId, simulatedUsersCount);
            var agentObjectId = ResourcePath.GetObjectId(
                instanceId,
                ResourceProviderNames.FoundationaLLM_Agent,
                AgentResourceTypeNames.Agents,
                "MockAgent");

            await Task.WhenAll(
                userIdentities
                .Select(userIdentity => SimulateAssistantUserContextCreation(
                    instanceId,
                    agentObjectId,
                    resourceProviders.AzureOpenAIResourceProvider,
                    userIdentity)));
        }

        private async Task SimulateAssistantUserContextCreation(
            string instanceId,
            string agentObjectId,
            IResourceProviderService resourceProvider,
            UnifiedUserIdentity userIdentity)
        {
            var assistantUserContextName = $"{userIdentity.UPN!.NormalizeUserPrincipalName()}-assistant-{instanceId.ToLower()}";
            //var assistantUserContext = new AssistantUserContext
            //{
            //    Name = assistantUserContextName,
            //    UserPrincipalName = userIdentity.UPN!,
            //    AgentAssistants = new()
            //    {
            //        {
            //            agentObjectId,
            //            new()
            //            {
            //                Endpoint = "endpoint_placeholder",
            //                ModelDeploymentName = "model_placeholder",
            //                Prompt = "prompt_placeholder",
            //            }
            //        }
            //    }
            //};

            //await resourceProvider.UpsertResourceAsync<AssistantUserContext, AzureOpenAIConversationMappingUpsertResult>(
            //    instanceId,
            //    assistantUserContext,
            //    userIdentity);
        }

        private List<UnifiedUserIdentity> GetUserIdentities(
            int hostId,
            int simulatedUsersCount)
        {
            return Enumerable.Range(1, 20)
                .Select(i => new UnifiedUserIdentity
                {
                    GroupIds = ["00000000-0000-0000-0000-000000000001"],
                    UserId = $"00000000-0000-0000-{hostId:D4}-{i:D12}",
                    Username = $"load_test_user_{hostId:D3}_{i:D3}@solliance.net",
                    Name = $"Load Test User {hostId:D3}-{i:D3}",
                    UPN = $"load_test_user_{hostId:D3}_{i:D3}@solliance.net"
                })
                .ToList();
        }
    }
}