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

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task Quota_API_Controller(
            string apiName,
            string controllerName,
            int userCount,
            int callCount,
            int callDelayMilliseconds,
            bool expectQuotaExceeded)
        {
            // Wait 5 seconds to ensure the APIRequestQuotaService is initialized.
            await Task.Delay(5000);

            WriteLine("============ FoundationaLLM Quota Local Enforcement - API Controller Tests ============");

            // Simulates {userCount} users simultaneously sending each a set of
            // {callCount} requests to the {controllerName} controller of the {apiName} API,
            // at intervals of {callDelayMilliseconds} milliseconds.
            var userWorkloads = Enumerable.Range(1, userCount)
                .Select(x => Task.Run(() => RunUserWorkload(apiName, controllerName, x, callCount, callDelayMilliseconds)))
                .ToArray();

            await Task.WhenAll(userWorkloads);

            var evaluationResults = userWorkloads.SelectMany(x => x.Result).ToArray();

            if (expectQuotaExceeded)
                // We expect to see at least one situation where the quota has been exceeded.
                Assert.Contains(evaluationResults, x => x.QuotaExceeded);
            else
                // We don't expect to see any cases where the quota has benn exceeded.
                Assert.DoesNotContain(evaluationResults, x => x.QuotaExceeded);
        }

        private QuotaMetricPartitionState[] RunUserWorkload(
            string apiName,
            string controllerName,
            int userIndex,
            int callCount,
            int callDelayMilliseconds)
        {
            var evaluationResults = new QuotaMetricPartitionState[callCount];

            for (int i = 0; i < callCount; i++)
            {
                // Each user is identified by a user identifier and a user principal name derived from the numeric user index.
                evaluationResults[i] = _quotaService.EvaluateRawRequestForQuota(
                    apiName,
                    controllerName,
                    new UnifiedUserIdentity()
                    {
                        UserId = $"00000000-0000-0000-0000-{userIndex:D12}",
                        UPN = $"johndoe_{userIndex:D3}@foundationallm.ai"
                    });

                Thread.Sleep(callDelayMilliseconds);
            }

            return evaluationResults;
        }

        public static TheoryData<string, string, int, int, int, bool> TestData =>
            new()
            {
                { "TestAPI01", "Completions", 10, 60, 400, true },
                { "TestAPI02", "Completions", 10, 40, 2000, false }
            };
    }
}