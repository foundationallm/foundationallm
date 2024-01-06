using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Settings
{
    /// <summary>
    /// Types of authentication for blob storage accounts.
    /// </summary>
    public enum BlobStorageAuthenticationTypes
    {
        /// <summary>
        /// Unknown authentication type.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Azure managed identity authentication type.
        /// </summary>
        AzureIdentity,

        /// <summary>
        /// Connection string authentication type.
        /// </summary>
        ConnectionString,

        /// <summary>
        /// Account key authentication type.
        /// </summary>
        AccountKey
    }
}
