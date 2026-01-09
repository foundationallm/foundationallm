using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Core.Services.Providers
{
    /// <summary>
    /// Factory for creating realtime speech providers based on model configuration.
    /// </summary>
    public class RealtimeSpeechProviderFactory : IRealtimeSpeechProviderFactory
    {
        private readonly ILogger<AzureOpenAIRealtimeSpeechProvider> _azureOpenAILogger;

        public RealtimeSpeechProviderFactory(
            ILogger<AzureOpenAIRealtimeSpeechProvider> azureOpenAILogger)
        {
            _azureOpenAILogger = azureOpenAILogger;
        }

        public IRealtimeSpeechProvider CreateProvider(RealtimeSpeechAIModel model)
        {
            // For now, we only support Azure OpenAI Realtime
            // In the future, this can be extended to support other providers
            // based on model properties or endpoint configuration
            return new AzureOpenAIRealtimeSpeechProvider(_azureOpenAILogger);
        }
    }
}
