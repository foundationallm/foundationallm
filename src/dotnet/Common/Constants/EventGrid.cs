namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Types of Azure Event Grid events.
    /// </summary>
    public static class EventGridEventTypes
    {
        /// <summary>
        /// The type of event emitted by an Azure storage account when a blob is created/updated.
        /// </summary>
        public const string Microsoft_Storage_BlobCreated = "Microsoft.Storage.BlobCreated";
    }

    /// <summary>
    /// Azure Event Grid topic names.
    /// </summary>
    public static class EventGridTopics
    {
        /// <summary>
        /// The name of the FoundationaLLM storage Azure Event Grid topic.
        /// </summary>
        public const string FoundationaLLM_Storage = "storage";
    }

    /// <summary>
    /// Azure Event Grid topic subscription names.
    /// </summary>
    public static class EventGridTopicSubscriptions
    {
        /// <summary>
        /// The name of the FoundationaLLM Core API subscription to the [storage] Azure Event Grid topic.
        /// </summary>
        public const string FoundationaLLM_Storage_Core = "storage-core";
    }
}
