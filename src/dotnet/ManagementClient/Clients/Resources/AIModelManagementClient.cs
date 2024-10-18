using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;

namespace FoundationaLLM.Client.Management.Clients.Resources
{
    internal class AIModelManagementClient(IManagementRESTClient managementRestClient) : IAIModelManagementClient
    {
        /// <inheritdoc/>
        public async Task<List<ResourceProviderGetResult<AIModelBase>>> GetAIModelsAsync() =>
            await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<AIModelBase>>>(
                ResourceProviderNames.FoundationaLLM_AIModel,
                AIModelResourceTypeNames.AIModels
            );

        /// <inheritdoc/>
        public async Task<ResourceProviderGetResult<AIModelBase>> GetAIModelAsync(string aiModelName)
        {
            var result = await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<AIModelBase>>>(
                ResourceProviderNames.FoundationaLLM_AIModel,
                $"{AIModelResourceTypeNames.AIModels}/{aiModelName}"
            );

            if (result == null || result.Count == 0)
            {
                throw new Exception($"AI Model '{aiModelName}' not found.");
            }

            var agent = result[0];

            return agent;
        }

        /// <inheritdoc/>
        public async Task<ResourceProviderUpsertResult> UpsertAIModel(AIModelBase aiModel) =>
            await managementRestClient.Resources.UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_AIModel,
                $"{AIModelResourceTypeNames.AIModels}/{aiModel.Name}",
                aiModel
            );
    }
}
