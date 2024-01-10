﻿using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Services.RequestSources
{
    /// <summary>
    /// Implements a cache of <see cref="IRequestSourceService"/> objects as defined by <see cref="IRequestSourcesCache"/>.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the cache.
    /// </remarks>
    /// <param name="vectorizationWorkerOptions">The <see cref="IOptions"/> instance containing the <see cref="VectorizationWorkerSettings"/> instance.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create new loggers for child objects.</param>
    public class RequestSourcesCache(
        IOptions<VectorizationWorkerSettings> vectorizationWorkerOptions,
        IConfigurationSection queuesConfiguration,
        ILoggerFactory loggerFactory) : IRequestSourcesCache
    {
        private readonly Dictionary<string, IRequestSourceService> _requestSources = (new RequestSourcesBuilder())
                .WithSettings(vectorizationWorkerOptions.Value.RequestSources)
                .WithQueuing(vectorizationWorkerOptions.Value.QueuingEngine)
                .WithLoggerFactory(loggerFactory)
                .WithQueuesConfiguration(queuesConfiguration)
                .Build();

        /// <inheritdoc/>
        public Dictionary<string, IRequestSourceService> RequestSources =>
            _requestSources;
    }
}
