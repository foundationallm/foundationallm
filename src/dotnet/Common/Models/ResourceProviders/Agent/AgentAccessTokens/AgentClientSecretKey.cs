using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.Authorization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentAccessTokens
{
    /// <summary>
    /// Represents a secret key used as an agent access token to be shared to or received from the client.
    /// </summary>
    public class AgentClientSecretKey : ClientSecretKey
    {
        /// <summary>
        /// The name of the agent this key is associated with.
        /// </summary>
        public string? AgentName
        {
            get
            {
                var parts = ContextId.Split('~');
                if (parts.Length != 4
                    || parts.Any(p => string.IsNullOrWhiteSpace(p)))
                    return null;

                if (!StringComparer.OrdinalIgnoreCase.Equals(parts[0], InstanceId)
                    || !StringComparer.OrdinalIgnoreCase.Equals(parts[1], ResourceProviderNames.FoundationaLLM_Agent)
                    || !StringComparer.OrdinalIgnoreCase.Equals(parts[2], AgentResourceTypeNames.Agents))
                    return null;

                return parts[3];
            }
        }

        /// <summary>
        /// Sets the context identifier of this key based on the given agent name.
        /// </summary>
        /// <param name="agentName"></param>
        public void SetContextId(string agentName) =>
            ContextId = $"{InstanceId}~{ResourceProviderNames.FoundationaLLM_Agent}~{AgentResourceTypeNames.Agents}~{agentName}";

        /// <summary>
        /// Creates an <see cref="AgentClientSecretKey"/> instance from the given <see cref="ClientSecretKey"/> instance.
        /// </summary>
        /// <param name="clientSecretKey">The <see cref="ClientSecretKey"/> instance used to create the new instance.</param>
        /// <returns>An <see cref="AgentClientSecretKey"/> instance.</returns>
        public static AgentClientSecretKey FromClientSecretKey(ClientSecretKey clientSecretKey) =>
            new()
            {
                InstanceId = clientSecretKey.InstanceId,
                ContextId = clientSecretKey.ContextId,
                Id = clientSecretKey.Id,
                ClientSecret = clientSecretKey.ClientSecret
            };
    }
}
