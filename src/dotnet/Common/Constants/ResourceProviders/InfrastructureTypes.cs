namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Contains constants of the types of resources managed by the FoundationaLLM.Infrastructure resource provider.
    /// </summary>
    public static class InfrastructureTypes
    {
        /// <summary>
        /// Azure Container Apps Environment type.
        /// </summary>
        public const string AzureContainerAppsEnvironment = "azure-container-apps-environment";

        /// <summary>
        /// Azure Container App type.
        /// </summary>
        public const string AzureContainerApp = "azure-container-app";

        /// <summary>
        /// Azure Kubernetes Service type.
        /// </summary>
        public const string AzureKubernetesService = "azure-kubernetes-service";

        /// <summary>
        /// Azure Kubernetes Service Deployment type.
        /// </summary>
        public const string AzureKubernetesServiceDeployment = "azure-kubernetes-service-deployment";
    }
}
