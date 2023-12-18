    using FoundationaLLM.Vectorization.Models;
using System.Threading.Tasks;
using System;
using FoundationaLLM.Vectorization.Interfaces;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services.RequestSources
{
    public class MemoryRequestSourceService : IRequestSourceService
    {
        private readonly string _sourceName;
        private readonly ILogger<MemoryRequestSourceService> _logger;
        private readonly ConcurrentQueue<VectorizationRequest> _requests = new ConcurrentQueue<VectorizationRequest>();

        /// <inheritdoc/>
        public string SourceName => _sourceName;

        public MemoryRequestSourceService(
            string sourceName,
            ILogger<MemoryRequestSourceService> logger)
        {
            _sourceName = sourceName;
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task DeleteRequest(string requestId) => Task.CompletedTask;

        /// <inheritdoc/>
        public Task<bool> HasRequests() =>
            Task.FromResult(_requests.Count > 0);

        /// <inheritdoc/>
        public Task<IEnumerable<VectorizationRequest>> ReceiveRequests(int count)
        {
            var result = new List<VectorizationRequest>();

            for (int i = 0; i < count; i++)
            {
                if (_requests.TryDequeue(out var request))
                    result.Add(request);
                else
                    break;
            }
            
            return Task.FromResult<IEnumerable<VectorizationRequest>>(result);
        }

        /// <inheritdoc/>
        public Task SubmitRequest(VectorizationRequest request)
        {
            _requests.Enqueue(request);
            return Task.CompletedTask;
        }
    }
}
