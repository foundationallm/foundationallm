using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;

namespace FoundationaLLM.Client.Management.Interfaces
{
    /// <summary>
    /// Provides methods to manage AI model resources.
    /// </summary>
    public interface IAIModelManagementClient
    {
        /// <summary>
        /// Retrieves all AI model resources.
        /// </summary>
        /// <returns>All AI model resources to which the caller has access and which have not been marked as deleted.</returns>
        Task<List<ResourceProviderGetResult<AIModelBase>>> GetAIModelsAsync();

        /// <summary>
        /// Retrieves a specific AI model by name.
        /// </summary>
        /// <param name="aiModelName">The name of the AI model to retrieve.</param>
        /// <returns></returns>
        Task<ResourceProviderGetResult<AIModelBase>> GetAIModelAsync(string aiModelName);

        /// <summary>
        /// Upserts an AI model resource. If an AI model does not exist, it will be created.
        /// If an AI model configuration does exist, it will be updated.
        /// </summary>
        /// <param name="aiModel">The API endpoint configuration resource to create or update.</param>
        /// <returns>Returns a <see cref="ResourceProviderUpsertResult"/>, which contains the
        /// Object ID of the resource.</returns>
        Task<ResourceProviderUpsertResult> UpsertAIModel(AIModelBase aiModel);
    }
}
