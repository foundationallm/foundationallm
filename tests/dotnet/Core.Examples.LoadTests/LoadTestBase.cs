using FoundationaLLM.Core.Examples.LoadTests.Setup;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.LoadTests
{
    public class LoadTestBase(
        int virtualHostsCount,
        ITestOutputHelper output,
        TestFixture fixture) : TestBase(output, fixture)
    {
        private readonly int _virtualHostsCount = virtualHostsCount;

        override protected void ConfigureServiceProviders(TestFixture fixture, ITestOutputHelper output) =>
            fixture.ConfigureServiceProviders(
                _virtualHostsCount,
                output,
                (hostBuilder, testOutputHelper, virtualHostsCount) =>
                    LoadTestServicesInitializer.InitializeServices(
                        hostBuilder,
                        testOutputHelper,
                        virtualHostsCount));
    }
}
