namespace FoundationaLLM.Common.Models.Azure
{
    /// <summary>
    /// Provides information about an Azure OpenAI account.
    /// </summary>
    public class AzureOpenAIAccount
    {
        /// <summary>
        /// The name of the Azure OpenAI account.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The endpoint of the Azure OpenAI account.
        /// </summary>
        public required string Endpoint { get; set; }

        /// <summary>
        /// The list of <see cref="AzureOpenAIAccountDeployment"/> objects providing information about the deployments in the account.
        /// </summary>
        public List<AzureOpenAIAccountDeployment> Deployments { get; set; } = [];
    }
}
