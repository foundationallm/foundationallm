﻿using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration;

/// <summary>
/// The completion request object.
/// </summary>
public class CompletionRequest : CompletionRequestBase
{
    /// <summary>
    /// The name of the selected agent.
    /// </summary>
    [JsonPropertyName("agent_name")]
    public string? AgentName { get; set; }


    /// <summary>
    /// A list of Gatekeeper feature names used by the orchestration request.
    /// </summary>
    [JsonPropertyName("gatekeeper_options")]
    public string[]? GatekeeperOptions { get; set; }

    /// <summary>
    /// Settings that override some aspects of behaviour of the orchestration.
    /// </summary>
    [JsonPropertyName("settings")]
    public OrchestrationSettings? Settings { get; set; }
}
