using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0001

namespace FoundationaLLM.SemanticKernel.Core.Filters
{
    /// <summary>
    /// Provides the default behavior for filtering actions during prompt rendering.
    /// </summary>
    public class DefaultPromptFilter : IPromptFilter
    {
        /// <summary>
        /// The rendered prompt.
        /// </summary>
        public string RenderedPrompt => _renderedPrompt;

        private string _renderedPrompt = string.Empty;

        /// <inheritdoc/>
        public void OnPromptRendered(PromptRenderedContext context) =>
            _renderedPrompt = context.RenderedPrompt;

        /// <inheritdoc/>
        public void OnPromptRendering(PromptRenderingContext context)
        {
        }
    }
}
