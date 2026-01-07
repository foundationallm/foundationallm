using FoundationaLLM.Common.Models.ResourceProviders.AIModel;

namespace FoundationaLLM.Core.Interfaces
{
    /// <summary>
    /// Factory for creating realtime speech providers based on model configuration.
    /// </summary>
    public interface IRealtimeSpeechProviderFactory
    {
        /// <summary>
        /// Creates the appropriate provider for the given AI model.
        /// </summary>
        /// <param name="model">The AI model configuration.</param>
        /// <returns>A realtime speech provider instance.</returns>
        IRealtimeSpeechProvider CreateProvider(RealtimeSpeechAIModel model);
    }
}
