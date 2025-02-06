﻿using Azure.Core;
using Azure.Identity;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.Authentication;
using Microsoft.Identity.Web;
using System.IdentityModel.Tokens.Jwt;

namespace FoundationaLLM.Common.Authentication
{
    /// <summary>
    /// Provides the context of the current service.
    /// </summary>
    public static class ServiceContext
    {
        /// <summary>
        /// Initializes the service context.
        /// </summary>
        /// <param name="production">Indicates whether the environment is production or not.</param>
        /// <param name="serviceName">The service name.</param>
        /// <param name="scope">The scope used to retrieve the access token.</param>
        public static void Initialize(bool production, string serviceName, string scope = ScopeURIs.FoundationaLLM_Authorization)
        {
            Production = production;
            ServiceName = serviceName;

            AzureCredential = Production
                ? new ManagedIdentityCredential(Environment.GetEnvironmentVariable(EnvironmentVariables.AzureClientId))
                : new AzureCliCredential();

            var tokenResult = AzureCredential.GetToken(
                new([$"{scope}/.default"]),
                default);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(tokenResult.Token) as JwtSecurityToken;
            var id = token!.Claims.First(c => c.Type == ClaimConstants.Oid)?.Value
                ?? token.Claims.First(c => c.Type == ClaimConstants.ObjectId)?.Value
                ?? token.Claims.First(c => c.Type == ClaimConstants.NameIdentifierId)?.Value;

            ServiceIdentity = new UnifiedUserIdentity
            {
                Name = serviceName,
                UserId = id,
                UPN = $"{serviceName}-{id}",
                GroupIds = []
            };

            // Used when debugging locally, set UPN for URLException overrides.
            if(!Production)
            {
                var upn = token.Claims.First(c => c.Type == ClaimConstants.PreferredUserName)?.Value;
                ServiceIdentity.UPN = upn;
            }
        }

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        public static string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the environment we run in is production or not.
        /// </summary>
        public static bool Production {  get; set; }

        /// <summary>
        /// The default Azure credential to use for authentication.
        /// </summary>
        public static TokenCredential? AzureCredential { get; set; }

        /// <summary>
        /// The <see cref="UnifiedUserIdentity"/> of the service based on its managed identity."/>
        /// </summary>
        public static UnifiedUserIdentity? ServiceIdentity { get; set; }
    }
}
