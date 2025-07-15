using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Core.Examples.Setup;
using FoundationaLLM.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.DistributedTests
{
    /// <summary>
    /// Runs load tests on resource provider resource references.
    /// </summary>
    public class Example0001_ResourceProviderResourceReferences : TestBase, IClassFixture<TestFixture>
    {
        public Example0001_ResourceProviderResourceReferences(ITestOutputHelper output, TestFixture fixture)
			: base(2, output, fixture, new DependencyInjectionContainerInitializer())
        {
        }

        [Fact]
        public async Task RunAsync()
        {
            WriteLine("============ FoundationaLLM Resource Provider Load Test ============");

            var simulatedUsersCount = 20;

            await Task.WhenAll(
                Enumerable.Range(1, _serviceContainerCount)
                .Select(i => SimulateServiceHostLoad(
                    i,
                    simulatedUsersCount,
                    ServiceContainers[i].ServiceProvider))
                );
        }

        private async Task SimulateServiceHostLoad(
            int hostId,
            int simulatedUsersCount,
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

            var resourceProviders = serviceProvider
                .GetRequiredService<IEnumerable<IResourceProviderService>>()
                .ToDictionary(x => x.Name);

            await Task.WhenAll(
                userIdentities
                .Select(userIdentity => SimulateAssistantUserContextCreation(
                    instanceId,
                    agentObjectId,
                    resourceProviders[ResourceProviderNames.FoundationaLLM_AzureOpenAI],
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