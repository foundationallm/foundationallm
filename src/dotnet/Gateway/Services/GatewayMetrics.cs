using System.Diagnostics.Metrics;

namespace FoundationaLLM.Gateway.Services
{
    public class GatewayMetrics
    {
        private readonly Counter<long> _textChunksEmbeddingsCount;
        private readonly Counter<long> _textChunksEmbeddingsSizeTokens;

        private readonly Counter<long> _textChunksCompletionsCount;
        private readonly Counter<long> _textChunksCompletionsSizeTokens;

        public GatewayMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("FoundationaLLM.Gateway");
            _textChunksEmbeddingsCount = meter.CreateCounter<long>(
                "foundationallm.gateway.embedding.text_chunks_count",
                description: "The number of text chunks submitted to embedding operations in the FoundationaLLM Gateway API.");
            _textChunksEmbeddingsSizeTokens = meter.CreateCounter<long>(
                "foundationallm.gateway.embedding.text_chunks_size_tokens",
                description: "The number of tokens in the text chunks submitted to embedding operations in the FoundationaLLM Gateway API.");
            _textChunksCompletionsCount = meter.CreateCounter<long>(
                "foundationallm.gateway.completion.text_chunks_count",
                description: "The number of text chunks submitted to completion operations in the FoundationaLLM Gateway API.");
            _textChunksCompletionsSizeTokens = meter.CreateCounter<long>(
                "foundationallm.gateway.completion.text_chunks_size_tokens",
                description: "The number of tokens in the text chunks submitted to completion operations in the FoundationaLLM Gateway API.");
        }

        public void IncrementTextChunkMeters(long embeddingsCount, long completionsCount)
        {
            if (embeddingsCount > 0)
                _textChunksEmbeddingsCount.Add(embeddingsCount);

            if (completionsCount > 0)
                _textChunksCompletionsCount.Add(completionsCount);
        }

        public void IncrementTextChunksSizeTokens(long embeddingsSize, long completionsSize)
        {
            if (embeddingsSize > 0)
                _textChunksEmbeddingsSizeTokens.Add(embeddingsSize);

            if (completionsSize > 0)
                _textChunksCompletionsSizeTokens.Add(completionsSize);
        }
    }
}
