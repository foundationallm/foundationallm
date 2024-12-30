namespace FoundationaLLM.Orchestration.Core.Models.ConfigurationOptions
{
    /// <summary>
    /// The settings configuration options for a LangChain Service
    /// </summary>
    public class LangChainServiceSettings
    {
        /// <summary>
        /// The polling interval in seconds to check the status of the LangChain service.
        /// </summary>
        public double PollingIntervalSeconds { get; set; } = 10;

    }
}
