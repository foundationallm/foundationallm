namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Defines the interface for a data pipeline resource provider client.
    /// </summary>
    public interface IResourceProviderClient
    {
        /// <summary>
        /// Sets the resource providers collection.
        /// </summary>
        /// <remarks>
        /// This property must be exposed by services that cannot be injected with the resource providers.
        /// These are background services that both provide and consume services to and from the resource providers.
        /// </remarks>
        public IEnumerable<IResourceProviderService> ResourceProviders { set; }
    }
}
