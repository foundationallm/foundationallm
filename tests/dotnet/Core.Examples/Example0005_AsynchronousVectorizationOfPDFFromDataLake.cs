using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Models;
using FoundationaLLM.Core.Examples.Setup;
using FoundationaLLM.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for running asynchronous vectorization over a PDF file in the Azure Data Lake storage account.
    /// Expects the following configuration values:
    ///     FoundationaLLM:DataSources:datalake_vectorization_input:AuthenticationType
    ///     FoundationaLLM:DataSources:datalake_vectorization_input:AccountName
    /// Expects the following document in the storage account:
    ///     /vectorization-input/SDZWA-Journal-January-2024.pdf
    /// References:
    ///     PDF public source: https://sandiegozoowildlifealliance.org/Journal/january-2024
    /// </summary>
    public class Example0005_AsynchronousVectorizationOfPDFFromDataLake: TestBase, IClassFixture<TestFixture>
    {
        private readonly IVectorizationTestService _vectorizationTestService;
        private readonly InstanceSettings _instanceSettings;
        private readonly string containerName = "vectorization-input";        
        private readonly string blobName = "SDZWA-Journal-January-2024.pdf";
        private readonly string dataSourceName = "datalake_vectorization_input";
        private readonly string dataSourceObjectId = String.Empty;
        private readonly string textPartitioningProfileName = "text_partition_profile";
        private readonly string textEmbeddingProfileName = "text_embedding_profile_generic";
        private readonly string indexingProfileName = "indexing_profile_pdf";
        private readonly string searchString = "Kurt and Ollie";
        private readonly string id = String.Empty;
        private readonly BlobStorageServiceSettings? _settings;

        public Example0005_AsynchronousVectorizationOfPDFFromDataLake(ITestOutputHelper output, TestFixture fixture)
            : base(1, output, fixture, new DependencyInjectionContainerInitializer())
        {
            _vectorizationTestService = GetService<IVectorizationTestService>();
            _instanceSettings = _vectorizationTestService.InstanceSettings;
            dataSourceObjectId = $"/instances/{_instanceSettings.Id}/providers/FoundationaLLM.DataSource/dataSources/{dataSourceName}";
            id = Guid.NewGuid().ToString();
            //_settings = GetService<IOptionsMonitor<BlobStorageServiceSettings>>()
            //        .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Vectorization_Storage);
        }

        [Fact]
        public async Task RunAsync()
        {
            WriteLine("============ Asynchronous Vectorization of a PDF from Data Lake ============");
            await RunExampleAsync();
        }

        private async Task RunExampleAsync()
        {
            string accountNameAppConfigKey = $"FoundationaLLM:DataSources:{dataSourceName}:AccountName";
            string authenticationTypeAppConfigKey = $"FoundationaLLM:DataSources:{dataSourceName}:AuthenticationType";

            try
            {
                WriteLine($"Create the App Configuration key {accountNameAppConfigKey}");
                await _vectorizationTestService.CreateAppConfiguration(
                    new AppConfigurationKeyValue
                    {
                        Name = accountNameAppConfigKey,
                        Key = accountNameAppConfigKey,
                        Value = _settings!.AccountName,
                        ContentType = ""
                    }
                );

                WriteLine($"Create the App Configuration key {authenticationTypeAppConfigKey}");
                await _vectorizationTestService.CreateAppConfiguration(
                    new AppConfigurationKeyValue
                    {
                        Name = authenticationTypeAppConfigKey,
                        Key = authenticationTypeAppConfigKey,
                        Value = "AzureIdentity",
                        ContentType = ""
                    }
                );

                WriteLine($"Create the data source: {dataSourceName} via the Management API");
                await _vectorizationTestService.CreateDataSource(dataSourceName);

                Thread.Sleep(5000); // processing too quickly, pause after the creation of the data source

                WriteLine($"Create the vectorization text partitioning profile: {textPartitioningProfileName} via the Management API");
                await _vectorizationTestService.CreateTextPartitioningProfile(textPartitioningProfileName);

                WriteLine($"Create the vectorization text embedding profile: {textEmbeddingProfileName} via the Management API");
                await _vectorizationTestService.CreateTextEmbeddingProfile(textEmbeddingProfileName);

                WriteLine($"Create the vectorization indexing profile: {indexingProfileName} via the Management API");
                await _vectorizationTestService.CreateIndexingProfile(indexingProfileName);

                ContentIdentifier ci = new()
                {
                    DataSourceObjectId = dataSourceObjectId,
                    MultipartId =
                    [
                        $"{_settings!.AccountName}.blob.core.windows.net",
                        containerName,
                        blobName
                    ],
                    CanonicalId = $"{containerName}/{(blobName[..blobName.LastIndexOf('.')])}"
                };

                WriteLine($"Create the vectorization request: {id} via the Management API");
                List<VectorizationStep> steps =
                [
                    new VectorizationStep { Id = "extract", Parameters = [] },
                    new VectorizationStep { Id = "partition", Parameters = [] },
                    new VectorizationStep { Id = "embed", Parameters = [] },
                    new VectorizationStep { Id = "index", Parameters = [] },
                ];
                var request = new VectorizationRequest
                {
                    RemainingSteps = ["extract", "partition", "embed", "index"],
                    CompletedSteps = [],
                    ProcessingType = VectorizationProcessingType.Asynchronous,
                    ContentIdentifier = ci,
                    Name = id,
                    Steps = steps,
                    ObjectId = $"{VectorizationResourceTypeNames.VectorizationRequests}/{id}"
                };
                //Create the vectorization request, re-assign the fully qualified object id if desired.
                request.ObjectId = await _vectorizationTestService.CreateVectorizationRequest(request);

                WriteLine($"Verify the vectorization request {id} was created by retrieving it from the Management API");
                var resource = await _vectorizationTestService.GetVectorizationRequest(request)
                    ?? throw new Exception("Vectorization request failed creation. Invalid result was returned.");
                WriteLine($"Issue the process action on the vectorization request: {id} via the Management API");
                var vectorizationResult = await _vectorizationTestService.ProcessVectorizationRequest(request)
                    ?? throw new Exception("Vectorization request failed to complete successfully. Invalid result was returned.");
                if (vectorizationResult.IsSuccess == false)
                {
                    //retrieve more verbose error logging from resource....
                    resource = await _vectorizationTestService.GetVectorizationRequest(request);
                    throw new Exception($"Vectorization request failed to complete successfully. Message(s):\n{string.Join("\n", resource.ErrorMessages)}");
                }

                WriteLine($"Get the initial processing state for the vectorization request: {id} via the Management API");
                //As this is an asynchronous request, poll the status of the vectorization request until it has completed (or failed). Retrieve initial state.
                resource = await _vectorizationTestService.GetVectorizationRequest(request);

                // The finalized state of the vectorization request is either "Completed" or "Failed"
                // Give it a max of 10 minutes to complete, then exit loop and fail the test.
                WriteLine($"Polling the processing state of the async vectorization request: {id} by retrieving the request from the Management API");
                int timeRemainingMilliseconds = 600000;
                var pollDurationMilliseconds = 30000; //poll duration of 30 seconds
                while (resource.ProcessingState != VectorizationProcessingState.Completed && resource.ProcessingState != VectorizationProcessingState.Failed && timeRemainingMilliseconds > 0)
                {
                    Thread.Sleep(pollDurationMilliseconds);
                    timeRemainingMilliseconds -= pollDurationMilliseconds;
                    resource = await _vectorizationTestService.GetVectorizationRequest(request);
                }

                if (resource.ProcessingState == VectorizationProcessingState.Failed)
                    throw new Exception($"Vectorization request failed to complete successfully. Error Messages:\n{string.Join("\n", resource.ErrorMessages)}");

                if (timeRemainingMilliseconds <= 0)
                    throw new Exception("Vectorization request failed to complete successfully. Timeout exceeded.");

                WriteLine($"Vectorization request: {id} completed successfully.");

                //perform a search - this PDF yields 27 documents
                WriteLine($"Verify a search yields 27 documents.");
                TestSearchResult result = await _vectorizationTestService.QueryIndex(indexingProfileName, textEmbeddingProfileName, searchString);
                if (result.QueryResult.TotalCount != 27)
                    throw new Exception($"Query did not return the expected number of query results. Expected: 27, Retrieved: {result.QueryResult.TotalCount}");
                if (result.VectorResults.TotalCount != 27)
                    throw new Exception($"Query did not return the expected number of vector results. Expected: 27, Retrieved: {result.VectorResults.TotalCount}");
            }
            catch (Exception ex)
            {
                WriteLine($"Exception: {ex.Message}");
                throw;
            }
            finally
            {
                WriteLine($"Delete the App Configuration key {accountNameAppConfigKey}");
                await _vectorizationTestService.DeleteAppConfiguration(accountNameAppConfigKey);

                WriteLine($"Delete the App Configuration key {authenticationTypeAppConfigKey}");
                await _vectorizationTestService.DeleteAppConfiguration(authenticationTypeAppConfigKey);

                WriteLine($"Delete the data source: {dataSourceName} via the Management API");
                await _vectorizationTestService.DeleteDataSource(dataSourceName);

                WriteLine($"Delete the vectorization text partitioning profile: {textPartitioningProfileName} via the Management API");
                await _vectorizationTestService.DeleteTextPartitioningProfile(textPartitioningProfileName);

                WriteLine($"Delete the vectorization text embedding profile: {textEmbeddingProfileName} via the Management API");
                await _vectorizationTestService.DeleteTextEmbeddingProfile(textEmbeddingProfileName);

                WriteLine($"Delete the vectorization indexing profile: {indexingProfileName} via the Management API along with the index");
                await _vectorizationTestService.DeleteIndexingProfile(indexingProfileName, true);
            }
        }
    }
}
