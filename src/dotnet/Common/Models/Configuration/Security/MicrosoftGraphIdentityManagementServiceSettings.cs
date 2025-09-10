using FoundationaLLM.Common.Services.Security;

namespace FoundationaLLM.Common.Models.Configuration.Security

{
    /// <summary>
    /// Provides settings for the <see cref="MicrosoftGraphIdentityManagementService" service./>
    /// </summary>
    public class MicrosoftGraphIdentityManagementServiceSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to retrieve the on-premises account name for users.
        /// </summary>
        public bool RetrieveOnPremisesAccountName { get; set; }
    }
}
