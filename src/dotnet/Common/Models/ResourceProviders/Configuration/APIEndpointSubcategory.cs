namespace FoundationaLLM.Common.Models.ResourceProviders.Configuration
{
    /// <summary>
    /// The subcategory for api endpoint class.
    /// </summary>
    public enum APIEndpointSubcategory
    {
        /// <summary>
        /// Denotes the subcategory for OneDrive Work or School for the FileStoreConnector category.
        /// </summary>
        OneDriveWorkSchool,

        /// <summary>
        /// Denotes the subcategory for indexing services.
        /// </summary>
        Indexing,

        /// <summary>
        /// Denotes the subcategory for AI model endpoints, such as Azure OpenAI.
        /// </summary>
        AIModel
    }
}
