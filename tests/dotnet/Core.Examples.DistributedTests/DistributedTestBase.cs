using FoundationaLLM.Core.Examples.DistributedTests.Setup;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.DistributedTests
{
    public class DistributedTestBase(
        int virtualHostsCount,
        ITestOutputHelper output,
        TestFixture fixture) : TestBase(output, fixture)
    {
        private readonly int _virtualHostsCount = virtualHostsCount;

        protected override List<IServiceProvider> GetServiceProviders(ITestOutputHelper output, TestFixture fixture) =>
            LoadTestServicesInitializer.InitializeServices(
                fixture.HostBuilder,
                output,
                _virtualHostsCount);
    }
}
