using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FoundationaLLM.Management.Models.Configuration
{
    /// <summary>
    /// Contains settings for the Azure App Configuration service.
    /// </summary>
    public record AppConfigurationSettings
    {
        /// <summary>
        /// The Azure App Configuration connection string.
        /// </summary>
        public required string ConnectionString { get; set; }
    }
}
