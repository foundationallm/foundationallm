using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipeline.Interfaces;

namespace FoundationaLLM.DataPipeline.Clients
{
    /// <summary>
    /// Client for the Data Pipeline API.
    /// </summary>
    /// <param name="httpClientFactoryService">The HTTP client factory used to create HTTP clients.</param>
    public class RemoteDataPipelineServiceClient(
        IHttpClientFactoryService httpClientFactoryService
        ) : IDataPipelineServiceClient
    {
        private readonly IHttpClientFactoryService _httpClientFactory = httpClientFactoryService;
        private HttpClient? _httpClient;

        /// <summary>
        /// Creates a new instance of the <see cref="RemoteDataPipelineServiceClient"/> class.
        /// </summary>
        /// <param name="httpClientFactoryService">The HTTP client factory used to create HTTP clients.</param>
        /// <returns>The newly created instance of the class.</returns>
        public static async Task<RemoteDataPipelineServiceClient> CreateAsync(
            IHttpClientFactoryService httpClientFactoryService)
        {
            var client = new RemoteDataPipelineServiceClient(httpClientFactoryService);
            await client.InitializeAsync();
            return client;
        }

        /// <summary>
        /// Initializes the instance by creating an HTTP client.
        /// </summary>
        public async Task InitializeAsync() =>
            _httpClient = await _httpClientFactory.CreateClient(
                HttpClientNames.DataPipelineAPI,
                ServiceContext.ServiceIdentity!);

        public async Task<DataPipelineRun> CreateDataPipelineRunAsync(DataPipelineRun dataPipelineRun) => throw new NotImplementedException();

        public async Task<DataPipelineRun> GetDataPipelineRunAsync(string dataPipelineRunId) => throw new NotImplementedException();
    }
}
