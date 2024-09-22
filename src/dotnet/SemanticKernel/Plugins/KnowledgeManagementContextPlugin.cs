#pragma warning disable SKEXP0001

using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Vectorization;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using System.ComponentModel;
using System.Text.Json;

namespace FoundationaLLM.SemanticKernel.Core.Plugins
{
    /// <summary>
    /// Provides the capability to build a context by recalling memories using vector search.
    /// </summary>
    public class KnowledgeManagementContextPlugin(
        ISemanticTextMemory memory,
        string indexName)
    {
        private readonly ISemanticTextMemory _memory = memory;
        private readonly string _indexName = indexName;

        /// <summary>
        /// Builds the context used for chat completions.
        /// </summary>
        /// <example>
        /// <param name="userPrompt">The input text to find related memories for.</param>
        [KernelFunction(name: "BuildContext")]
        public async Task<string> BuildContextAsync(
            [Description("The user prompt for which the context is being built.")] string userPrompt,
            [Description("The history of messages in the current conversation.")] List<MessageHistoryItem> messageHistory)
        {
            var memories = await _memory
                .SearchAsync(_indexName, userPrompt, 5, 0.25)
                .ToListAsync()
                .ConfigureAwait(false);

            var recommendations = memories
                .Select(m => new
                {
                    Source = JsonSerializer.Deserialize<ContentIdentifier>(m.Metadata.AdditionalMetadata)!.MultipartId[1],
                    m.Metadata.Text
                })
                .ToList();

            return JsonSerializer.Serialize(recommendations);
        }
    }
}
