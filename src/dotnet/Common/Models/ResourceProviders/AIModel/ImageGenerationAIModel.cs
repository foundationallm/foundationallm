using FoundationaLLM.Common.Constants.ResourceProviders;

namespace FoundationaLLM.Common.Models.ResourceProviders.AIModel
{
    /// <summary>
    /// Provides the properties for AI models used for image generation.
    /// </summary>
    public class ImageGenerationAIModel :AIModelBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ImageGenerationAIModel"/> AI model.
        /// </summary>
        public ImageGenerationAIModel() =>
            Type = AIModelTypes.ImageGeneration;
    }
}
