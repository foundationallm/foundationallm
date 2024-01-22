using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Interfaces
{
    /// <summary>
    /// Calls endpoints available on all hub API services.
    /// </summary>
    public interface IHubAPIService
    {
        /// <summary>
        /// Gets the status of the Hub API service.
        /// </summary>
        /// <returns></returns>
        Task<string> Status();
    }
}
