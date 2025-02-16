﻿namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Registry of built-in tool names.
    /// </summary>
    public static class ToolNames
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
    }
}
