using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentTemplates;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Defines the contract for working with agent templates.
    /// </summary>
    public interface IAgentTemplateService
    {
        /// <summary>
        /// Sets the collection of resource providers to be used by the service.
        /// </summary>
        /// <remarks>Using this method is required to avoid circular references, since this service
        /// is a dependency for the FoundationaLLM.Agent resource provider while it also requires
        /// a refrence to the same resource provider.</remarks>
        /// <param name="resourceProviderServices">A collection of resource provider services to register.</param>
        void SetResourceProviderServices(
            IEnumerable<IResourceProviderService> resourceProviderServices);

        /// <summary>
        /// Creates a new agent based on the provided parameter values and template files.
        /// </summary>
        /// <remarks>This method performs an asynchronous operation to create an agent based on the
        /// provided parameter values and template files. Ensure that all required template files are included in
        /// <paramref name="agentTemplateFiles"/> and that the <paramref name="createAgentRequest"/> contains valid
        /// parameters.</remarks>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="createAgentRequest">The request object containing the parameter values for creating the agent.</param>
        /// <param name="agentTemplateFiles">A dictionary of template file names and their corresponding content, used to define the agent's structure
        /// and behavior.</param>
        /// <param name="userIdentity">The identity of the user requesting to create the agent.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="ResourceProviderUpsertResult"/>  indicating the outcome of the agent creation process.</returns>
        Task<ResourceProviderUpsertResult> CreateAgent(
            string instanceId,
            AgentCreationFromTemplateRequest createAgentRequest,
            Dictionary<string, string> agentTemplateFiles,
            UnifiedUserIdentity userIdentity);
    }
}
