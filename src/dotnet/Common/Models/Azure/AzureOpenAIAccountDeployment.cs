namespace FoundationaLLM.Common.Models.Azure
{
    /// <summary>
    /// Provides information about a model deployment in an Azure OpenAI account.
    /// </summary>
    public class AzureOpenAIAccountDeployment
    {
        /// <summary>
        /// The endpoint of the account in which the model is deployed.
        /// </summary>
        public required string AccountEndpoint { get; set; }

        /// <summary>
        /// The name of the deployment.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The name of the model in the deployment.
        /// </summary>
        public required string ModelName { get; set; }

        /// <summary>
        /// The version of the model in the deployment.
        /// </summary>
        public required string ModelVersion { get; set; }

        /// <summary>
        /// The limit placed on requests sent to the model.
        /// </summary>
        public int RequestRateLimit { get; set; }

        /// <summary>
        /// The period (in seconds) to which <see cref="RequestRateLimit"/> applies.
        /// </summary>
        public int RequestRateRenewalPeriod { get; set; }

        /// <summary>
        /// The limit placed on tokens sent to the model.
        /// </summary>
        public int TokenRateLimit { get; set; }

        /// <summary>
        /// The period (in seconds) to which <see cref="TokenRateLimit"/> applies.
        /// </summary>
        public int TokenRateRenewalPeriod { get; set; }

        /// <summary>
        /// The capabilities of the model in the deployment.
        /// </summary>
        public IReadOnlyDictionary<string, string>? Capabilities { get; set; }
    }
}
