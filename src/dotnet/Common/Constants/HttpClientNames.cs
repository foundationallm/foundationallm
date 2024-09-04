using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.Common.Constants;

/// <summary>
/// Name constants used to configure and retrieve API endpoint configurations.
/// </summary>
public static class HttpClientNames
{
    /// <summary>
    /// Named client with matching configuration for the Core API.
    /// </summary>
    public const string CoreAPI = "CoreAPI";

    /// <summary>
    /// Named client with matching configuration for the Gatekeeper API.
    /// </summary>
    public const string GatekeeperAPI = "GatekeeperAPI";

    /// <summary>
    /// Named client with matching configuration for the Gatekeeper Integration API.
    /// </summary>
    public const string GatekeeperIntegrationAPI = "GatekeeperIntegrationAPI";

    /// <summary>
    /// Named client with matching configuration for the Orchestration API.
    /// </summary>
    public const string OrchestrationAPI = "OrchestrationAPI";

    /// <summary>
    /// Named client with matching configuration for the LangChain API.
    /// </summary>
    public const string LangChainAPI = "LangChainAPI";

    /// <summary>
    /// Named client with matching configuration for the Semantic Kernel API.
    /// </summary>
    public const string SemanticKernelAPI = "SemanticKernelAPI";

    /// <summary>
    /// Named client with matching configuration for the Agent Hub API.
    /// </summary>
    public const string AgentHubAPI = "AgentHubAPI";

    /// <summary>
    /// Named client with matching configuration for the Prompt Hub API.
    /// </summary>
    public const string PromptHubAPI = "PromptHubAPI";

    /// <summary>
    /// Named client with matching configuration for the DataSource Hub API.
    /// </summary>
    public const string DataSourceHubAPI = "DataSourceHubAPI";

    /// <summary>
    /// Named client with matching configuration for the Vectorization API.
    /// </summary>
    public const string VectorizationAPI = "VectorizationAPI";

    /// <summary>
    /// Named client with matching configuration for the Vectorization Worker.
    /// </summary>
    public const string VectorizationWorker = "VectorizationWorker";

    /// <summary>
    /// Named client with matching configuration for the Authorization API.
    /// </summary>
    public const string AuthorizationAPI = "AuthorizationAPI";

    /// <summary>
    /// Named client with matching configuration for the Management API.
    /// </summary>
    public const string ManagementAPI = "ManagementAPI";

    /// <summary>
    /// Named client with matching configuration for the Gateway API.
    /// </summary>
    public const string GatewayAPI = "GatewayAPI";

    /// <summary>
    /// Named client with matching configuration for the Gateway Adapter API.
    /// </summary>
    public const string GatewayAdapterAPI = "GatewayAdapterAPI";

    /// <summary>
    /// Named client with matching configuration for the Azure Content Safety service.
    /// </summary>
    public const string AzureContentSafety = "AzureContentSafety";

    /// <summary>
    /// Named client with matching configuration for the Enkrypt Guardrails service.
    /// </summary>
    public const string EnkryptGuardrails = "EnkryptGuardrails";

    /// <summary>
    /// Named client with matching configuration for the Lakera Guard service.
    /// </summary>
    public const string LakeraGuard = "LakeraGuard";

    /// <summary>
    /// Named client with matching configuration for the Azure Event Grid service.
    /// </summary>
    public const string AzureEventGrid = "AzureEventGrid";

    /// <summary>
    /// Named client with matching configuration for the State API.
    /// </summary>
    public const string StateAPI = "StateAPI";

    /// <summary>
    /// Named client with matching configuration for the OneDrive API.
    /// </summary>
    public const string OneDriveAPI = "OneDriveAPI";

    /// <summary>
    /// All HTTP client names.
    /// </summary>
    public readonly static string[] All = [
        CoreAPI,
        GatekeeperAPI,
        GatekeeperIntegrationAPI,
        OrchestrationAPI,
        LangChainAPI,
        SemanticKernelAPI,
        AgentHubAPI,
        PromptHubAPI,
        DataSourceHubAPI,
        VectorizationAPI,
        VectorizationWorker,
        AuthorizationAPI,
        ManagementAPI,
        GatewayAPI,
        GatewayAdapterAPI,
        AzureContentSafety,
        EnkryptGuardrails,
        LakeraGuard,
        AzureEventGrid,
        StateAPI
    ];

    /// <summary>
    /// All HTTP client names used in <see cref="IDownstreamAPIService"/> implementations.
    /// </summary>
    public readonly static string[] AllDownstream = [
        GatekeeperAPI,
        GatekeeperIntegrationAPI,
        OrchestrationAPI,
        AgentHubAPI,
        PromptHubAPI,
        DataSourceHubAPI
    ];
}
