using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Configuration.Authentication
{
    /// <summary>
    /// Represents settings specific to Entra.
    /// </summary>
    public class EntraSettings
    {
        /// <summary>
        /// The scopes associated with the Entra settings.
        /// </summary>
        public string? Scopes { get; set; }
    }
}
