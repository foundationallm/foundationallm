using FoundationaLLM.Common.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Models.Agents;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Stores context information for the current HTTP request.
    ///
    /// The CurrentUserIdentity stores a <see cref="UnifiedUserIdentity"/> object
    /// resolved from one or more services.
    /// </summary>
    public interface ICallContext
    {
        /// <summary>
        /// The current <see cref="UnifiedUserIdentity"/> object resolved
        /// from one or more services.
        /// </summary>
        UnifiedUserIdentity? CurrentUserIdentity { get; set; }
        /// <summary>
        /// The unique identifier of the current FoundationaLLM deployment instance.
        /// </summary>
        string? InstanceId { get; set; }
    }
}
