using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.Orchestration.Core.Services
{
    /// <summary>
    /// Manages internal and external orchestration services.
    /// </summary>
    public class LLMOrchestrationServiceManager : ILLMOrchestrationServiceManager
    {
        private readonly InstanceSettings _instanceSettings;
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LLMOrchestrationServiceManager> _logger;

        private List<string> _externalOrchestrationServiceNames = [];

        /// <summary>
        /// Creates a new instance of the LLM Orchestration Service Manager.
        /// </summary>
        /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
        /// <param name="resourceProviderServices">A list of <see cref="IResourceProviderService"/> resource providers.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve configuration values.</param>
        /// <param name="logger">The logger for the orchestration service manager.</param>
        public LLMOrchestrationServiceManager(
            IOptions<InstanceSettings> instanceOptions,
            IEnumerable<IResourceProviderService> resourceProviderServices,
            IConfiguration configuration,
            ILogger<LLMOrchestrationServiceManager> logger)
        {
            _instanceSettings = instanceOptions.Value;
            _resourceProviderServices =
                resourceProviderServices.ToDictionary<IResourceProviderService, string>(rps => rps.Name);
            _configuration = configuration;
            _logger = logger;

            // Kicks off the initialization on a separate thread and does not wait for it to complete.
            // The completion of the initialization process will be signaled by setting the _initialized property.
            _ = Task.Run(Initialize);
        }

        #region Initialization

        /// <summary>
        /// Performs the initialization of the orchestration service.
        /// </summary>
        /// <returns></returns>
        private async Task Initialize()
        {
            try
            {
                _logger.LogInformation("Starting to initialize the LLM Orchestration Service Manager service...");

                var configurationResourceProvider = _resourceProviderServices[ResourceProviderNames.FoundationaLLM_Configuration];
                await configurationResourceProvider.WaitForInitialization();

                var apiEndpointConfigurations = await configurationResourceProvider.GetResourcesAsync<APIEndpointConfiguration>(
                    _instanceSettings.Id,
                    ServiceContext.ServiceIdentity!);

                _externalOrchestrationServiceNames = apiEndpointConfigurations
                    .Where(aec => aec.Resource.Category == APIEndpointCategory.ExternalOrchestration
                        && aec.Resource.AuthenticationParameters.TryGetValue(AuthenticationParametersKeys.APIKeyConfigurationName, out var apiKeyConfigObj)
                        && apiKeyConfigObj is JsonElement apiKeyConfig
                        && !string.IsNullOrWhiteSpace(apiKeyConfig.GetString())
                        && apiKeyConfig.GetString()!.StartsWith(AppConfigurationKeySections.FoundationaLLM_APIEndpoints))
                    .Select(aec => aec.Resource.Name)
                    .ToList();

                _logger.LogInformation("The LLM Orchestration Service Manager service was successfully initialized.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing the LLM Orchestration Service Manager service.");
            }
        }

        #endregion

        /// <inheritdoc/>
        public async Task<List<ServiceStatusInfo>> GetAggregateStatus(string instanceId, IServiceProvider serviceProvider)
        {
            var result = new List<ServiceStatusInfo>();

            var serviceStatuses = GetOrchestrationServices(serviceProvider)
                .ToAsyncEnumerable()
                .Select(async (ILLMOrchestrationService x, CancellationToken ct) => await x.GetStatus(instanceId));

            await foreach (var serviceStatus in serviceStatuses)
                result.Add(serviceStatus);

            return result;
        }

        /// <inheritdoc/>
        public ILLMOrchestrationService GetService(string instanceId, string serviceName, IServiceProvider serviceProvider, IOrchestrationContext callContext)
        {
            var internalOrchestrationService = serviceProvider.GetServices<ILLMOrchestrationService>()
                .SingleOrDefault(srv => srv.Name == serviceName);

            if (internalOrchestrationService != null)
                return internalOrchestrationService;

            if (_externalOrchestrationServiceNames.Contains(serviceName))
                return new LLMOrchestrationService(
                    serviceName,
                    serviceProvider.GetRequiredService<ILogger<LLMOrchestrationService>>(),
                    serviceProvider.GetRequiredService<IHttpClientFactoryService>(),
                    callContext);

            throw new OrchestrationException($"The LLM orchestration service {serviceName} is not available.");
        }

        private IEnumerable<ILLMOrchestrationService> GetOrchestrationServices(IServiceProvider serviceProvider) =>
            serviceProvider.GetServices<ILLMOrchestrationService>()
                .Where(llmSrv =>
                    llmSrv.GetType() == typeof(LangChainService)
                    || llmSrv.GetType() == typeof(SemanticKernelService))
                .Concat(
                    _externalOrchestrationServiceNames.Select(eosn =>
                        new LLMOrchestrationService(
                            eosn,
                            serviceProvider.GetRequiredService<ILogger<LLMOrchestrationService>>(),
                            serviceProvider.GetRequiredService<IHttpClientFactoryService>(),
                            serviceProvider.GetRequiredService<IOrchestrationContext>())));
    }
}
