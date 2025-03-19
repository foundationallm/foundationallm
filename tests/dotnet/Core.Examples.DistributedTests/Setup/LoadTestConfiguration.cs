using FoundationaLLM.Core.Examples.Setup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace FoundationaLLM.Core.Examples.DistributedTests.Setup
{
    /// <summary>
    /// Configures the test services for the examples.
    /// </summary>
    public sealed class LoadTestConfiguration : TestConfigurationBase
    {
		private LoadTestConfiguration(IHostApplicationBuilder builder)
			: base((IConfigurationRoot)builder.Configuration)
        {
        }
	}
}
