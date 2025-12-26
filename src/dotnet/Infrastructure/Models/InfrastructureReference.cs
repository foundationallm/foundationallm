using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Infrastructure;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Infrastructure.Models
{
    /// <summary>
    /// Contains a reference to an Infrastructure resource.
    /// </summary>
    public class InfrastructureReference : ResourceReference
    {
        /// <summary>
        /// The object type of the infrastructure resource.
        /// </summary>
        [JsonIgnore]
        public override Type ResourceType =>
            Type switch
            {
                InfrastructureTypes.AzureContainerAppsEnvironment => typeof(AzureContainerAppsEnvironment),
                InfrastructureTypes.AzureContainerApp => typeof(AzureContainerApp),
                InfrastructureTypes.AzureKubernetesService => typeof(AzureKubernetesService),
                InfrastructureTypes.AzureKubernetesServiceDeployment => typeof(AzureKubernetesServiceDeployment),
                _ => throw new ResourceProviderException($"The infrastructure resource type {Type} is not supported.")
            };
    }
}
