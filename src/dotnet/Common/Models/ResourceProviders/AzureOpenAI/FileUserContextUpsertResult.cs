namespace FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI
{
    /// <summary>
    /// Represents the result of an upsert operation for an <see cref="FileUserContext"/> object.
    /// </summary>
    public class FileUserContextUpsertResult : ResourceProviderUpsertResult<FileUserContext>
    {
        /// <summary>
        /// The identifier of the newly created OpenAI assistant file.
        /// </summary>
        public required string NewOpenAIFileId { get; set; }
    }
}
