using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Core.Examples.Setup;
using FoundationaLLM.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Concepts.Purge
{
    /// <summary>
    /// Example class for testing embedding via the Gateway API.
    /// </summary>
    public class Example_Purge_AgentResources(
        ITestOutputHelper output,
        TestFixture fixture) : TestBase(1, output, fixture, new DependencyInjectionContainerInitializer()), IClassFixture<TestFixture>
    {
        private IConfiguration _configuration = null!;

        [Fact]
        public async Task Purge_AgentResources_OrphanedAssistants()
        {
            var agentResourceProviderService = GetService<IEnumerable<IResourceProviderService>>()
                .Single(rp => rp.Name == ResourceProviderNames.FoundationaLLM_Agent);
            var instanceSettings = GetService<IOptions<InstanceSettings>>().Value;

            WriteLine("============ FoundationaLLM Purge - Agent Resources Tests ============");

            var agents = await agentResourceProviderService.GetResourcesAsync<AgentBase>(
                instanceSettings.Id,
                ServiceContext.ServiceIdentity!);

            Assert.True(agents.Any(), "No agents found for the instance.");
        }
    }
}