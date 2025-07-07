namespace FoundationaLLM.Context.Models.Configuration
{
    /// <summary>
    /// Provides settings for the FoundationaLLM Context API.
    /// </summary>
    public class ContextServiceSettings
    {
        /// <summary>
        /// Gets or sets the file service settings.
        /// </summary>
        public required FileServiceSettings FileService { get; set; }

        /// <summary>
        /// Gets or sets the knowledge graph service settings.
        /// </summary>
        public required KnowledgeServiceSettings KnowledgeService { get; set; }
    }
}
