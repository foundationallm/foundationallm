namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Registry of built-in tool names.
    /// </summary>
    public static class AgentToolNames
    {
        /// <summary>
        /// Name of the built-in tool OpenAIAssistantsFileSearch.
        /// </summary>
        public const string OpenAIAssistantsFileSearchTool = "OpenAIAssistantsFileSearch";

        /// <summary>
        /// Name of the built-in tool for the OpenAI Assistants Code Interpreter.
        /// </summary>
        public const string OpenAIAssistantsCodeInterpreterTool = "OpenAIAssistantsCodeInterpreter";

        /// <summary>
        /// Name of the built-in tool for DALL-E image generation.
        /// </summary>
        public const string DALLEImageGenerationTool = "DALLEImageGeneration";

        /// <summary>
        /// The name of the FoundationaLLM code interpreter tool.
        /// </summary>
        public const string FoundationaLLMCodeInterpreterTool = "Code";

        /// <summary>
        /// The name of the FoundationaLLM knowledge search tool.
        /// </summary>
        public const string FoundationaLLMKnowledgeSearchTool = "Knowledge";
    }
}
