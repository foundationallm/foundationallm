namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// The names of the actions implemented by most of the FoundationaLLM resource providers.
    /// </summary>
    public static class ResourceProviderActions
    {
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
    }
}
