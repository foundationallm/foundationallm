namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Contains constants for the types of resources managed by the FoundationaLLM.AzureOpenAI resource provider.
    /// </summary>
    public static class AzureOpenAITypes
    {
        /// <summary>
        /// OpenAI assistant resources associated with a FoundationaLLM user.
        /// </summary>
        public const string AssistantUserContext = "assistant-user-context";

        /// <summary>
        /// OpenAI assistant files associated with a FoundationaLLM user.
        /// </summary>
        public const string FileUserContext = "file-user-context";
    }
}
