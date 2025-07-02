namespace FoundationaLLM.Context.Interfaces
{
    /// <summary>
    /// Defines the service interface for the FoundationaLLM Knowledge Graph service.
    /// </summary>
    public interface IKnowledgeGraphService
    {
        /// <summary>
        /// Updates the knowledge graph with the specified entities and relationships source files.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="knowledgeGraphId">The knowledge graph identifier.</param>
        /// <param name="entitiesSourceFilePath">The file path of the source entities file.</param>
        /// <param name="relationshipsSourceFilePath">The file path of the source relationships file.</param>
        /// <returns></returns>
        Task UpdateKnowledgeGraph(
            string instanceId,
            string knowledgeGraphId,
            string entitiesSourceFilePath,
            string relationshipsSourceFilePath);
    }
}
