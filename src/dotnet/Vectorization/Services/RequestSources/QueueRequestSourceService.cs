using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Services.RequestSources
{
    public class QueueRequestSourceService : IRequestSourceService
    {
        private readonly string _sourceName;
        private readonly ILogger<QueueRequestSourceService> _logger;

        /// <inheritdoc/>
        public string SourceName => _sourceName;

        public QueueRequestSourceService(
            string sourceName,
            ILogger<QueueRequestSourceService> logger)
        {
            _sourceName = sourceName;
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task DeleteRequest(string requestId) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<bool> HasRequests() => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<IEnumerable<VectorizationRequest>> ReceiveRequests(int count) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task SubmitRequest(VectorizationRequest request) => throw new NotImplementedException();
    }
}
