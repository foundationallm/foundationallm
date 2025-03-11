using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for running the default FoundationaLLM agent completions in both session and sessionless modes.
    /// </summary>
    public class Example_Quota_LocalEnforcement : BaseTest, IClassFixture<TestFixture>
    {
        private readonly IQuotaService _apiRequestQuotaService;

        public Example_Quota_LocalEnforcement(ITestOutputHelper output, TestFixture fixture)
            : base(output, [fixture.ServiceProvider])
        {
            _apiRequestQuotaService = GetService<IQuotaService>();
        }

        [Fact]
        public async Task RunAsync()
        {
            WriteLine("============ FoundationaLLM Quota Enforcement - Local ============");

            // Wait 5 seconds to ensure the APIRequestQuotaService is initialized.
            await Task.Delay(5000);

            await RunExampleAsync();
        }

        private async Task RunExampleAsync()
        {
            for (int i = 0; i < 120; i++)
            {
                var evaluationResult = _apiRequestQuotaService.EvaluateRawRequestForQuota(
                    ServiceNames.CoreAPI,
                    "Completion",
                    new UnifiedUserIdentity()
                    {
                        UserId = Guid.Empty.ToString(),
                        UPN = "johndoe@foundationallm.ai"
                    });

                Assert.False(evaluationResult.QuotaExceeded);

                await Task.Delay(500);
            }
        }
    }
}