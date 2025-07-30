using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.DataPipelineEngine.Services;
using FoundationaLLM.Tests.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples.Concepts.Storage
{
    /// <summary>
    /// Example class for testing embedding via the Gateway API.
    /// </summary>
    public class Example_Storage_DataPipelineStateStorage
    {
        private readonly ITestOutputHelper _output;
        private readonly TestEnvironment _testEnvironment;

        private IDataPipelineStateService _dataPipelineStateService = null!;

        public Example_Storage_DataPipelineStateStorage(
            ITestOutputHelper output)
        {
            _output = output;
            _testEnvironment = new TestEnvironment();
            _testEnvironment.InitializeServices(output);
        }

        private void InitializeServices()
        {
            var storageService = new BlobStorageService(
                storageOptions: Options.Create<BlobStorageServiceSettings>(new BlobStorageServiceSettings
                {
                    AuthenticationType = AuthenticationTypes.AzureIdentity,
                    AccountName = _testEnvironment.Configuration["DataPipelineStateService:StorageAccountName"]
                }),
                logger: _testEnvironment.GetRequiredService<ILogger<BlobStorageService>>());

            _dataPipelineStateService = new DataPipelineStateService(
                null!,
                storageService,
                _testEnvironment.GetRequiredService<ILogger<DataPipelineStateService>>());
        }

        [Fact]
        public async Task Storage_DataPipelineStateService_Load()
        {
            InitializeServices();

            _output.WriteLine("============ FoundationaLLM Storage - Data Pipeline State Service Tests ============");

            
        }
    }
}