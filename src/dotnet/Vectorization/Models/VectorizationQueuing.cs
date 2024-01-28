using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Models
{
    /// <summary>
    /// Types of queuing engines used to dispatch vectorization requests.
    /// </summary>
    public enum VectorizationQueuing
    {
        /// <summary>
        /// No persisted queuing. Results in using a simple, non-production grade, in-memory queuing mechanism.
        /// </summary>
        None,
        /// <summary>
        /// Azure storage account queuing.
        /// </summary>
        AzureStorageQueue
    }
}
