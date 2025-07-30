using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.Core.Examples.Setup;
using FoundationaLLM.DataPipelineEngine.Models.Configuration;
using FoundationaLLM.Tests;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Concepts.Storage
{
    /// <summary>
    /// Example class for testing embedding via the Gateway API.
    /// </summary>
    public class Example_Storage_BlobStorageService(
        ITestOutputHelper output,
        TestFixture fixture) : TestBase(1, output, fixture, new DependencyInjectionContainerInitializer()), IClassFixture<TestFixture>
    {
        private IStorageService _storageService = null!;
        private IConfiguration _configuration = null!;

        [Fact]
        public async Task Storage_BlobStorageService_VeryLargeBlob()
        {
            InitializeStorageClient();

            WriteLine("============ FoundationaLLM Storage - Blob Storage Service Tests ============");

            // Generate 1000 random bytes
            var randomBytes = new byte[512 * 1024 * 1024];
            new Random().NextBytes(randomBytes);

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _storageService.WriteFileAsync(
                    "test",
                    $"test-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.parquet",
                    new MemoryStream(randomBytes),
                    "application/vnd.apache.parquet",
                    default);
            });
        }

        private void InitializeStorageClient()
        {
            _configuration = GetService<IConfiguration>();

            _storageService = new BlobStorageService(
                storageOptions: Options.Create<BlobStorageServiceSettings>(
                    GetService<IOptions<DataPipelineStateServiceSettings>>().Value.Storage),
                logger: GetService<ILogger<BlobStorageService>>());
        }
    }
}