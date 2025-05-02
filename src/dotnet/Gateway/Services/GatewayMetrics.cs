using System.Diagnostics.Metrics;

namespace FoundationaLLM.Gateway.Services
{
    public class GatewayMetrics
    {
        private readonly Counter<long> _textChunksEmbeddedCount;
        private readonly Counter<long> _textChunksEmbeddedSizeTokens;

        public GatewayMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("FoundationaLLM.Gateway");
            _textChunksEmbeddedCount = meter.CreateCounter<long>(
                "foundationallm.gateway.embedding.text_chunks_count",
                description: "The number of text chunks embedded in the FoundationaLLM Gateway API.");
            _textChunksEmbeddedSizeTokens = meter.CreateCounter<long>(
                "foundationallm.gateway.embedding.text_chunks_size_tokens",
                description: "The number of tokens in the text chunks embedded in the FoundationaLLM Gateway API.");
        }

        public void IncrementTextChunksEmbedded(long count) =>
            _textChunksEmbeddedCount.Add(count);

        public void IncrementTextChunksSizeTokens(long size) =>
            _textChunksEmbeddedSizeTokens.Add(size);
    }
}
