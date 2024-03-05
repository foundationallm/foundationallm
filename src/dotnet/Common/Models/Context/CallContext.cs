using FoundationaLLM.Common.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Agents;

namespace FoundationaLLM.Common.Models.Context
{
    /// <inheritdoc/>
    public class CallContext : ICallContext
    {
        /// <inheritdoc/>
        public UnifiedUserIdentity? CurrentUserIdentity { get; set; }
        /// <inheritdoc/>
        public string? InstanceId { get; set; }
    }
}
