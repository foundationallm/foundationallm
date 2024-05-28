    using FoundationaLLM.Vectorization.Models;
using System.Threading.Tasks;
using System;
using FoundationaLLM.Vectorization.Interfaces;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using FoundationaLLM.Vectorization.Models.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;

namespace FoundationaLLM.Vectorization.Services.RequestSources
{
    /// <summary>
    /// Implements an in-memory request source, suitable for testing and quick prototyping.
    /// </summary>
    /// <param name="settings">The settings used to initialize the request source.</param>
    /// <param name="logger">The logger instnce used for logging.</param>
    public class MemoryRequestSourceService(
        RequestSourceServiceSettings settings,
        ILogger<MemoryRequestSourceService> logger) : IRequestSourceService
    {
        private readonly RequestSourceServiceSettings _settings = settings;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ILogger<MemoryRequestSourceService> _logger = logger;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly ConcurrentQueue<VectorizationRequest> _requests = new();

        /// <inheritdoc/>
        public string SourceName => _settings.Name;

        /// <inheritdoc/>
        public Task<bool> HasRequests() =>
            Task.FromResult(!_requests.IsEmpty);

        /// <inheritdoc/>
        public Task<IEnumerable<(VectorizationRequest Request, string MessageId, string PopReceipt, long DequeueCount)>> ReceiveRequests(int count)
        {
            var result = new List<(VectorizationRequest, string, string, long)>();

            for (int i = 0; i < count; i++)
            {
                if (_requests.TryDequeue(out var request))                    
                    result.Add(new (request, string.Empty, string.Empty, 0));
                else
                    break;
            }
            
            return Task.FromResult<IEnumerable<(VectorizationRequest, string, string, long)>>(result);
        }

        /// <inheritdoc/>
        public Task DeleteRequest(string requestId, string popReceipt) => Task.CompletedTask;

        /// <inheritdoc/>
        public Task SubmitRequest(VectorizationRequest request)
        {
            _requests.Enqueue(request);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task UpdateRequest(string requestId, string popReceipt, VectorizationRequest request)
        {
            _requests.Single(r => r.Name == request.Name).ErrorCount = request.ErrorCount;
            return Task.CompletedTask;
        }
    }
}
