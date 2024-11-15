namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// Provides the parameter names for the FoundationaLLM.Conversation resource provider get operations.
    /// </summary>
    public static class ConversationResourceProviderGetParameterNames
    {
        /// <summary>
        /// The name of the parameter that specifies the conversation type.
        /// </summary>
        /// <remarks>
        /// The value of the parameter must be one of the values defined in <see cref="ConversationTypes"/>.
        /// </remarks>
        public const string ConversationType = "conversation-type";
    }
}
