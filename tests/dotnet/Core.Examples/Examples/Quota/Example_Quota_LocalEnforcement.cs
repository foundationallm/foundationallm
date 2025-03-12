using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Quota;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for running the default FoundationaLLM agent completions in both session and sessionless modes.
    /// </summary>
    public class Example_Quota_LocalEnforcement : TestBase, IClassFixture<TestFixture>
    {
        private readonly IQuotaService _quotaService;

        public Example_Quota_LocalEnforcement(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture)
        {
            _quotaService = GetService<IQuotaService>();
        }

        [Fact]
        public async Task RunAsync()
        {
            // Wait 5 seconds to ensure the APIRequestQuotaService is initialized.
            await Task.Delay(5000);

            WriteLine("============ FoundationaLLM Quota Enforcement - Local ============");

            await RunExampleAsync();
        }

        private async Task RunExampleAsync()
        {
            int callCount = 60;
            int callDelayMilliseconds = 400;
            int threadCount = 10;

            var userWorkloads = Enumerable.Range(0, threadCount)
                .Select(x => Task.Run(() => RunUserWorkload(x, callCount, callDelayMilliseconds)))
                .ToArray();

            await Task.WhenAll(userWorkloads);

            var evaluationResults = userWorkloads.SelectMany(x => x.Result).ToArray();
            Assert.Contains(evaluationResults, x => x.QuotaExceeded);
        }

        private QuotaEvaluationResult[] RunUserWorkload(
            int userId,
            int callCount,
            int callDelayMilliseconds)
        {
            var evaluationResults = new QuotaEvaluationResult[callCount];

            for (int i = 0; i < callCount; i++)
            {
                evaluationResults[i] = _quotaService.EvaluateRawRequestForQuota(
                    ServiceNames.CoreAPI,
                    "Completion",
                    new UnifiedUserIdentity()
                    {
                        UserId = $"00000000-0000-0000-0000-{userId:D12}",
                        UPN = $"johndoe_{userId:D3}@foundationallm.ai"
                    });

                Thread.Sleep(callDelayMilliseconds);
            }

            return evaluationResults;
        }
    }
}