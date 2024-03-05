namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Contains constants of the keys for all overridable model settings.
    /// </summary>
    public static class ModelSettingsKeys
    {
        /// <summary>
        /// The key name for the deployment_name model parameter.
        /// This value should be a string representing the name of the model deployment in Azure OpenAI.
        /// </summary>
        public const string Deployment_Name = "deployment_name";
        /// <summary>
        /// The key name for the temperature parameter.
        /// This value should be a float between 0.0 and 1.0.
        /// </summary>
        public const string Temperature = "temperature";
    }
}
