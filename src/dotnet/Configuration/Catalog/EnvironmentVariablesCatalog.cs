using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Configuration.Environment;

namespace FoundationaLLM.Configuration.Catalog
{
    /// <summary>
    /// Provides the catalog of environment variables required by the solution.
    /// </summary>
    public static class EnvironmentVariablesCatalog
    {
        private static readonly List<EnvironmentVariableEntry> Entries =
        [
            new EnvironmentVariableEntry(EnvironmentVariables.Hostname,
                "The Azure Container App or Azure Kubernetes Service hostname."),
            new EnvironmentVariableEntry(EnvironmentVariables.FoundationaLLM_Version,
                "The build version of the container. This is also used for the app version used to validate the minimum version of the app required to use certain configuration entries.")
        ];

        /// <summary>
        /// Returns the required environment variables.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EnvironmentVariableEntry> GetRequiredEnvironmentVariables() => Entries;
    }

}
