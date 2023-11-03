using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Common.Authentication
{
    /// <summary>
    /// Service filter attribute for X-API-Key header validation.
    /// </summary>
    public class APIKeyAuthenticationAttribute : ServiceFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the APIKeyAuthenticationAttribute class.
        /// </summary>
        public APIKeyAuthenticationAttribute()
            : base(typeof(APIKeyAuthenticationFilter))
        {
        }
    }
}
