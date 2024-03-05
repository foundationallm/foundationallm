using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Infrastructure
{
    /// <summary>
    /// Represents the status of a service.
    /// </summary>
    public class ServiceStatusInfo
    {
        /// <summary>
        /// The name of the service.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// The instance of the service.
        /// </summary>
        public string? Instance { get; set; }
        /// <summary>
        /// The deployed FoundationaLLM version of the service.
        /// </summary>
        public string? Version { get; set; }
        /// <summary>
        /// The status of the service.
        /// </summary>
        public string? Status { get; set; }
        /// <summary>
        /// The message associated with the status.
        /// </summary>
        public string? Message { get; set; }
    }
}
