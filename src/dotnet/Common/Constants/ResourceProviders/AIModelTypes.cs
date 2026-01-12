namespace FoundationaLLM.Common.Constants.ResourceProviders
{
    /// <summary>
    /// AI Model type contants.
    /// </summary>
    public static class AIModelTypes
    {
        /// <summary>
        /// Basic AIModel type without practical functionality. Used as base for all other model types.
        /// </summary>
        public const string Basic = "basic";
        /// <summary>
        /// Embedding model type
        /// </summary>
        public const string Embedding = "embedding";
        /// <summary>
        /// Completion model type
        /// </summary>
        public const string Completion = "completion";
        /// <summary>
        /// Image generation model type
        /// </summary>
        public const string ImageGeneration = "image-generation";
        /// <summary>
        /// Realtime speech-to-speech model type
        /// </summary>
        public const string RealtimeSpeech = "realtime-speech";
    }
}
