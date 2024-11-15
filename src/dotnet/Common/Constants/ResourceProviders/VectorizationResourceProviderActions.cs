namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// The names of the actions implemented by the Agent resource provider.
    /// </summary>
    public static class VectorizationResourceProviderActions
    {
        /// <summary>
        /// Activate a vectorization pipeline.
        /// </summary>
        public const string Activate = "activate";

        /// <summary>
        /// Deactivate a vectorization pipeline.
        /// </summary>
        public const string Deactivate = "deactivate";

        /// <summary>
        /// Process a vectorization request.
        /// </summary>
        public const string Process = "process";
    }
}
