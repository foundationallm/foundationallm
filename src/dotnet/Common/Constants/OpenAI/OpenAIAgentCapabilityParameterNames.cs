﻿namespace FoundationaLLM.Common.Constants.OpenAI
{
    /// <summary>
    /// Provides the names of the parameters that can be used to create OpenAI agent capabilities.
    /// </summary>
    /// <remarks>
    /// The constants are used by the callers of the <see cref="IGatewayServiceClient"/> implementations.
    /// </remarks>
    public static class OpenAIAgentCapabilityParameterNames
    {
        #region Requests

        /// <summary>
        /// Indicates the need to create a new OpenAI assistant.
        /// </summary>
        public const string CreateOpenAIAssistant = "OpenAI.Assistants.Assistant.Create";

        /// <summary>
        /// Indicates the need to create a new OpenAI assistant thread.
        /// </summary>
        public const string CreateOpenAIAssistantThread = "OpenAI.Assistants.Thread.Create";

        /// <summary>
        /// Indicates the need to create a new OpenAI assistant file.
        /// </summary>
        public const string CreateOpenAIFile = "OpenAI.File.Create";

        /// <summary>
        /// Indicates the need to add an existing OpenAI assistant file to the OpenAI assistant vector store.
        /// </summary>
        public const string AddOpenAIFileToVectorStore = "OpenAI.File.AddToVectorStore";

        #endregion

        #region Inputs

        /// <summary>
        /// Provides the prompt used by the OpenAI assistant.
        /// </summary>
        public const string OpenAIAssistantPrompt = "OpenAI.Assistants.Assistant.Prompt";

        /// <summary>
        /// Provides the Azure OpenAI endpoint used to manage Open AI assistants.
        /// </summary>
        public const string OpenAIEndpoint = "OpenAI.Endpoint";

        /// <summary>
        /// Provides the model deployment name used by the OpenAI assistant.
        /// </summary>
        public const string OpenAIModelDeploymentName = "OpenAI.ModelDeploymentName";

        /// <summary>
        /// The object identifier of the FoundationaLLM attachment resource.
        /// </summary>
        public const string AttachmentObjectId = "FoundationaLLM.Attachment.ObjectId";

        #endregion

        #region Outputs

        /// <summary>
        /// Provides the identifier of an existing OpenAI assistant.
        /// </summary>
        public const string OpenAIAssistantId = "OpenAI.Assistants.Assistant.Id";

        /// <summary>
        /// Provides the identifier of an existing OpenAI assistant thread.
        /// </summary>
        public const string OpenAIAssistantThreadId = "OpenAI.Assistants.Thread.Id";

        /// <summary>
        /// Provides the identifier of an existing OpenAI assistant file.
        /// </summary>
        public const string OpenAIFileId = "OpenAI.Files.Id";

        /// <summary>
        /// Provides the identifier of an existing OpenAI assistant vector store.
        /// </summary>
        public const string OpenAIVectorStoreId = "OpenAI.VectorStore.Id";

        /// <summary>
        /// Indicates whether the Open AI assistant file vectorization process completed successfully.
        /// </summary>
        public const string AddOpenAIFileToVectorStoreSuccess = "OpenAI.File.AddToVectorStoreSuccess";

        #endregion
    }
}
