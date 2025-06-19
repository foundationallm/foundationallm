using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.DataPipelines;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Core.Examples.Setup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Parquet.Serialization;
using Xunit.Abstractions;
using static System.Net.WebRequestMethods;

namespace FoundationaLLM.Core.Examples.Concepts.Gateway
{
    /// <summary>
    /// Example class for testing embedding via the Gateway API.
    /// </summary>
    public class Example_Gateway_Embedding : TestBase, IClassFixture<TestFixture>
    {
        private GatewayServiceClient _gatewayServiceClient = null!;

        public Example_Gateway_Embedding(ITestOutputHelper output, TestFixture fixture)
            : base(1, output, fixture)
        {
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task Gateway_Embedding_RateLimitEnforcemennt(
            string filePath,
            int maxChunksCount,
            int waitTimeSeconds,
            bool expectRateLimitExceeded)
        {
            // Wait for all required initialization tasks to complete.
            await Task.WhenAll([
                StartEventsWorkers(),
                InitializeGatewayServiceClient()
            ]);

            WriteLine("============ FoundationaLLM Gateway - Embedding Tests ============");

            var instanceSettings = GetService<IOptions<InstanceSettings>>().Value;
            var storageService = GetService<IStorageService>();

            var microsoftMLTokenizer = GetKeyedService<ITokenizerService>("MicrosoftML");
            var tryAGITokenizer = GetKeyedService<ITokenizerService>("TryAGI");

            var binaryContent = await storageService.ReadFileAsync(
                instanceSettings.Id,
                filePath,
                default);

            var contentItemParts = await ParquetSerializer.DeserializeAsync<DataPipelineContentItemPart>(
                binaryContent.ToStream());

            //Ensure we're moving into a new rate limit window.
            await Task.Delay(TimeSpan.FromSeconds(waitTimeSeconds));
            var embeddingResult = await _gatewayServiceClient.StartEmbeddingOperation(
                instanceSettings.Id,
                new TextEmbeddingRequest
                {
                    EmbeddingModelName = "text-embedding-3-large",
                    EmbeddingModelDimensions = 2048,
                    Prioritized = true,
                    TextChunks = [.. contentItemParts
                        .Take(maxChunksCount)
                        .Select(part => new TextChunk
                        {
                            Position = part.Position,
                            Content = part.Content,
                            TokensCount = part.ContentSizeTokens
                        })]
                });

            while (embeddingResult.InProgress)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                embeddingResult = await _gatewayServiceClient.GetEmbeddingOperationResult(
                    instanceSettings.Id,
                    embeddingResult.OperationId!);
            }

            Assert.Equal(expectRateLimitExceeded, embeddingResult.Failed);
        }

        private async Task InitializeGatewayServiceClient()
        {
            _gatewayServiceClient = new GatewayServiceClient(
                await GetService<IHttpClientFactoryService>().CreateClient(
                    ServiceNames.GatewayAPI,
                    ServiceContext.ServiceIdentity!),
                GetService<ILogger<GatewayServiceClient>>());
        }

        public static TheoryData<string, int, int, bool> TestData =>
        new()
        {
            //{
            //    "data-pipeline-state/DefaultFileUpload/ciprian-foundationallm-ai/run-20250616-114757-dUHfyUn2f0C-8zznlMvXXg-TAfGit69y0OhQOwAAtltKw/content-items/file-20250613-171735-iUxkmekq70a9pAN3sQHqRA/content-parts.parquet",
            //    831,
            //    0,
            //    false 
            //}
            {
                "data-pipeline-state/DefaultFileUpload/ciprian-foundationallm-ai/run-20250616-114757-dUHfyUn2f0C-8zznlMvXXg-TAfGit69y0OhQOwAAtltKw/content-items/file-20250613-171735-iUxkmekq70a9pAN3sQHqRA/content-parts.parquet",
                1000,
                0,
                false
            }
        };
    }
}