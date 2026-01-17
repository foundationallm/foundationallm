namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Contains constants for the custom actions supported by the FoundationaLLM.Skill resource provider.
    /// </summary>
    public static class SkillResourceProviderActions
    {
        /// <summary>
        /// Search for skills using semantic similarity.
        /// </summary>
        public const string Search = "search";

        /// <summary>
        /// Approve a skill that is pending approval.
        /// </summary>
        public const string Approve = "approve";
    }
}
