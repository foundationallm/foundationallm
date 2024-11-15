using FoundationaLLM.Core.Examples.Models;
using Microsoft.Extensions.Configuration;

namespace FoundationaLLM.Core.Examples.Setup
{
    /// <summary>
    /// Configures the test services for the examples.
    /// </summary>
    public sealed class TestConfiguration : TestConfigurationBase
    {
        public static CompletionQualityMeasurementConfiguration CompletionQualityMeasurementConfiguration => LoadSection<CompletionQualityMeasurementConfiguration>();
		public static SharePointVectorizationConfiguration SharePointVectorizationConfiguration => LoadSection<SharePointVectorizationConfiguration>();
		public static GenerateConversationsConfiguration GenerateConversationsConfiguration => LoadSection<GenerateConversationsConfiguration>();

		private TestConfiguration(IConfigurationRoot configRoot)
			: base(configRoot)
        {
        }
	}
}
