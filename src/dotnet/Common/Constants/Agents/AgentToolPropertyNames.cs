namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Provides well-known parameter names for agent tools.
    /// </summary>
    public static class AgentToolPropertyNames
    {
        /// <summary>
        /// Indicates whether the agent tool requires a code session.
        /// </summary>
        public const string CodeSessionRequired = "code_session_required";

        /// <summary>
        /// The name of the code session provider.
        /// </summary>
        public const string CodeSessionEndpointProvider = "code_session_endpoint_provider";

        /// <summary>
        /// User-level overrides for the code session endpoint provider.
        /// </summary>
        public const string CodeSessionEndpointProviderOverrides = "code_session_endpoint_provider_overrides";

        /// <summary>
        /// The programming language of the code session.
        /// </summary>
        public const string CodeSessionLanguage = "code_session_language";

        /// <summary>
        /// The identifier of the code session.
        /// </summary>
        public const string CodeSessionId = "code_session_id";

        /// <summary>
        /// The code session endpoint.
        /// </summary>
        public const string CodeSessionEndpoint = "code_session_endpoint";

        /// <summary>
        /// Represents the metadata filter key used in vector store operations.
        /// </summary>
        public const string VectorStoreMetadataFilter = "vector_store_metadata_filter";

        /// <summary>
        /// Represents the list of knowledge unit vector store metadata filters
        /// </summary>
        public const string KnowledgeUnitVectorStoreFilters = "knowledge_unit_vector_store_filters";

        /// <summary>
        /// Represents the maximum content size in characters for content items processed by data pipelines.
        /// </summary>
        public const string MaxContentSizeCharacters = "max_content_size_characters";
    }
}
