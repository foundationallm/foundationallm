namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// The names of the actions implemented by most of the FoundationaLLM resource providers.
    /// </summary>
    public static class ResourceProviderActions
    {
        /// <summary>
        /// Create a new resource.
        /// </summary>
        public const string CreateNew = "create-new";

        /// <summary>
        /// Check the validity of a resource name.
        /// </summary>
        public const string CheckName = "checkname";

        /// <summary>
        /// Purges a soft-deleted resource.
        /// </summary>
        public const string Purge = "purge";

        /// <summary>
        /// Filter resources.
        /// </summary>
        public const string Filter = "filter";

        /// <summary>
        /// Submit a query to a resource.
        /// </summary>
        public const string Query = "query";

        /// <summary>
        /// Set the knowledge graph associated with a knowledge unit.
        /// </summary>
        public const string SetGraph = "set-graph";

        /// <summary>
        /// Load the knowledge graph associated with a knowledge unit.
        /// </summary>
        public const string LoadGraph = "load-graph";

        /// <summary>
        /// Submit a request to render a knowledge graph.
        /// </summary>
        public const string RenderGraph = "render-graph";

        /// <summary>
        /// Load the content of a file.
        /// </summary>
        public const string LoadFileContent = "load-file-content";

        /// <summary>
        /// Validate resources.
        /// </summary>
        public const string Validate = "validate";

        /// <summary>
        /// Set the resource as the default.
        /// </summary>
        public const string SetDefault = "set-default";

        /// <summary>
        /// Set the resource owner.
        /// </summary>
        public const string SetOwner = "set-owner";

        /// <summary>
        /// Load a plugin package.
        /// </summary>
        public const string LoadPluginPackage = "load-plugin-package";

        /// <summary>
        /// Trigger the execution of a resource.
        /// </summary>
        public const string Trigger = "trigger";

        /// <summary>
        /// Activate a resource.
        /// </summary>
        public const string Activate = "activate";

        /// <summary>
        /// Deactivate a resource.
        /// </summary>
        public const string Deactivate = "deactivate";

        /// <summary>
        /// Trigger a resource command.
        /// </summary>
        public const string TriggerCommand = "trigger-command";

        /// <summary>
        /// Restart an infrastructure resource.
        /// </summary>
        public const string Restart = "restart";

        /// <summary>
        /// Scale an infrastructure resource.
        /// </summary>
        public const string Scale = "scale";
    }
}
