using FoundationaLLM.Common.Models.Orchestration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Interface for calling a downstream API.
    /// </summary>
    public interface IDownstreamAPIService
    {
        /// <summary>
        /// The name of the downstream API.
        /// </summary>
        public string APIName { get; }

        /// <summary>
        /// Gets a completion from the downstream API.
        /// </summary>
        /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
        /// <returns>The completion response.</returns>
        Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest);

        /// <summary>
        /// Gets a summary from the downstream API.
        /// </summary>
        /// <param name="summaryRequest">The summarize request containing the user prompt.</param>
        /// <returns>The summary response.</returns>
        Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest);
    }
}
