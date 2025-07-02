namespace FoundationaLLM.Plugins.DataPipeline.Constants
{
    /// <summary>
    /// Constants for the steps involved in building a knowledge graph within a data pipeline.
    /// </summary>
    public static class KnowledgeGraphBuildingSteps
    {
        /// <summary>
        /// The step for building the core structure of the knowledge graph.
        /// </summary>
        public const string CoreStructure = "CoreStructure";

        /// <summary>
        /// The step for summarizing entities within the knowledge graph.
        /// </summary>
        public const string EntitiesSummarization = "EntitiesSummarization";

        /// <summary>
        /// The step for summarizing relationships within the knowledge graph.
        /// </summary>
        public const string RelationshipsSummarization = "RelationshipsSummarization";

        /// <summary>
        /// The step for embedding entities within the knowledge graph.
        /// </summary>
        public const string EntitiesEmbedding = "EntitiesEmbedding";

        /// <summary>
        /// The step for embedding relationships within the knowledge graph.
        /// </summary>
        public const string RelationshipsEmbedding = "RelationshipsEmbedding";

        /// <summary>
        /// The step for publishing the knowledge graph to the Context API.
        /// </summary>
        public const string Publish = "Publish";
    }
}
