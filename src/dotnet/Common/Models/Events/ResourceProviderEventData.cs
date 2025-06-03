namespace FoundationaLLM.Common.Models.Events
{
    /// <summary>
    /// Represents the data for a resource provider event.
    /// </summary>
    public class ResourceProviderEventData
    {
        /// <summary>
        /// Gets or sets the UTC timestamp of the event.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the name of the service that is targeted by the event.
        /// </summary>
        public string? TargetServiceName { get; set; }

        /// <summary>
        /// Gets or sets the data associated with the event.
        /// </summary>
        public object? Data { get; set; }
    }
}
