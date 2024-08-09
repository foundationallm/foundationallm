﻿using FoundationaLLM.Common.Constants.ResourceProviders;

namespace FoundationaLLM.Common.Models.ResourceProviders.AIModel
{
    /// <summary>
    /// Provides the properties for AI models used for completions.
    /// </summary>
    public class CompletionAIModel : AIModelBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CompletionAIModel"/> AI model.
        /// </summary>
        public CompletionAIModel() =>
            Type = AIModelTypes.Completion;
    }
}
