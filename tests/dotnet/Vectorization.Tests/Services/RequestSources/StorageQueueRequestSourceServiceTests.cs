using Azure.Storage.Queues;
using FakeItEasy;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Configuration;
using FoundationaLLM.Vectorization.Services.RequestSources;
using Microsoft.Extensions.Logging;

namespace Vectorization.Tests.Services.RequestSources
{
    public class StorageQueueRequestSourceServiceTests
    {
        private StorageQueueRequestSourceService _storageQueueRequestSourceService;
        private QueueClient _queueClient;
        private IVectorizationStateService _stateService;
        
        public StorageQueueRequestSourceServiceTests()
        {
            _stateService = A.Fake<IVectorizationStateService>();
            RequestSourceServiceSettings requestManagerServiceSettings = new RequestSourceServiceSettings()
            {
                Name = Environment.GetEnvironmentVariable("StorageQueueServiceTestsQueueName") ?? "testing",
                AccountName = "Test_AccountName",
                VisibilityTimeoutSeconds = 60
            };
            _queueClient = new QueueServiceClient(new Uri($"https://test.dfs.core.windows.net")).GetQueueClient(requestManagerServiceSettings.Name);
            ILogger<StorageQueueRequestSourceService> logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<StorageQueueRequestSourceService>();
            _storageQueueRequestSourceService = new StorageQueueRequestSourceService(requestManagerServiceSettings, logger);
        }

        [Fact]
        public async void TestStorageQueueRequestSourceService()
        {
            await _queueClient.CreateAsync();

            await _storageQueueRequestSourceService.SubmitRequest(
                new VectorizationRequest {
                    Id = "d4669c9c-e330-450a-a41c-a4d6649abdef",
                    ContentIdentifier = new ContentIdentifier
                    {
                        MultipartId = new List<string> {
                            "https://somesa.blob.core.windows.net",
                            "vectorization-input",
                            "somedata.pdf"
                        },
                        DataSourceObjectId = "SomePDFData",
                        CanonicalId = "SomeBusinessUnit/SomePDFData"
                    },
                    ProcessingType = VectorizationProcessingType.Synchronous,
                    Steps = new List<VectorizationStep>
                    {
                        new VectorizationStep { Id = "embed", Parameters = new Dictionary<string, string> { } }
                    },
                    CompletedSteps = new List<string> { },
                    RemainingSteps = new List<string> { "embed" }
                }
            );

            Assert.True(await _storageQueueRequestSourceService.HasRequests());

            var vectorizationRequestQueueMessages = await _storageQueueRequestSourceService.ReceiveRequests(10);

            Assert.True(
                vectorizationRequestQueueMessages.Count() == 1
            );

            var vectorizationRequestQueueMessage = vectorizationRequestQueueMessages.FirstOrDefault();

            // Correct Deserialization
            Assert.Equal(
                "d4669c9c-e330-450a-a41c-a4d6649abdef",
                vectorizationRequestQueueMessage.Request.Id
            );

            // Message ID & Pop Receipt must be retained for deletion
            Assert.True(
                vectorizationRequestQueueMessage.MessageId != string.Empty
            );
            Assert.True(
                vectorizationRequestQueueMessage.PopReceipt != string.Empty
            );

            await _storageQueueRequestSourceService.DeleteRequest(
                vectorizationRequestQueueMessage.MessageId,
                vectorizationRequestQueueMessage.PopReceipt
            );

            Assert.False(await _storageQueueRequestSourceService.HasRequests());

            await _queueClient.DeleteAsync();
        }
    }
}
