using System.Text.Json.Serialization;
using System.Collections.Generic;
using FoundationaLLM.Common.Constants.Agents;

namespace FoundationaLLM.Common.Models.Orchestration.Request;

/// <summary>
/// OpenAI-compatible completion request object.
/// </summary>
public class OpenAIFacadeCompletionRequest
{
    /// <summary>
    /// ID of the model to use.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// The prompt to generate completions for.
    /// </summary>
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Generates best_of completions server-side and returns the "best" completion.
    /// </summary>
    [JsonPropertyName("best_of")]
    public int? BestOf { get; set; }

    /// <summary>
    /// Echo back the prompt in addition to the completion.
    /// </summary>
    [JsonPropertyName("echo")]
    public bool? Echo { get; set; }

    /// <summary>
    /// Return a list of the specified number of most likely tokens sorted by their logprobs.
    /// Maximum value is 5.
    /// </summary>
    [JsonPropertyName("logprobs")]
    public bool? LogProbs { get; set; }

    /// <summary>
    /// The number of completions to generate for each prompt.
    /// </summary>
    [JsonPropertyName("n")]
    public int? N { get; set; }

    /// <summary>
    /// If specified, the system will make a best effort to sample deterministically,
    /// such that repeated requests with the same seed and parameters should return
    /// the same result.
    /// </summary>
    [JsonPropertyName("seed")]
    public int? Seed { get; set; }

    /// <summary>
    /// Sequence where the API will stop generating further tokens.
    /// The returned text won't contain the stop sequence.
    /// </summary>
    [JsonPropertyName("stop")]
    public string? Stop { get; set; }

    /// <summary>
    /// The maximum number of tokens to generate in the completion.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; set; }

    /// <summary>
    /// What sampling temperature to use, between 0 and 2.
    /// Higher values like 0.8 will make the output more random,
    /// while lower values like 0.2 will make it more focused and deterministic.
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; set; }

    /// <summary>
    /// Sample using called nucleus sampling, where the model considers the results
    /// of the tokens with top_p probability mass.
    /// So 0.1 means only the tokens comprising the top 10% probability mass are considered.
    /// Higher values like 0.8 will make the output more random,
    /// while lower values like 0.2 will make it more focused and deterministic.
    /// </summary>
    [JsonPropertyName("top_p")]
    public float? TopP { get; set; }

    /// <summary>
    /// A unique identifier representing the end-user, which can help to monitor and detect abuse.
    /// </summary>
    [JsonPropertyName("user")]
    public string? User { get; set; }

    /// <summary>
    /// The suffix that comes after a completion of inserted text.
    /// </summary>
    [JsonPropertyName("suffix")]
    public string? Suffix { get; set; }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing
    /// frequency in the text so far, decreasing the model's likelihood to repeat the same line verbatim.
    /// </summary>
    [JsonPropertyName("frequency_penalty")]
    public float? FrequencyPenalty { get; set; }

    /// <summary>
    /// Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they
    /// appear in the text so far, increasing the model's likelihood to talk about new topics.
    /// </summary>
    [JsonPropertyName("presence_penalty")]
    public float? PresencePenalty { get; set; }

    /// <summary>
    /// Converts the OpenAI facade completion request to the internal CompletionRequest format.
    /// </summary>
    /// <returns>A CompletionRequest object populated with the facade request data.</returns>
    public CompletionRequest ToCompletionRequest()
    {
        var modelParameters = new Dictionary<string, object>();

        // Add parameters only if they are not null
        if (BestOf.HasValue)
            modelParameters[ModelParametersKeys.BestOf] = BestOf.Value;
        
        if (MaxTokens.HasValue)
            modelParameters[ModelParametersKeys.MaxTokens] = MaxTokens.Value;
        
        if (Temperature.HasValue)
            modelParameters[ModelParametersKeys.Temperature] = Temperature.Value;
        
        if (TopP.HasValue)
            modelParameters[ModelParametersKeys.TopP] = TopP.Value;
        
        if (FrequencyPenalty.HasValue)
            modelParameters[ModelParametersKeys.FrequencyPenalty] = FrequencyPenalty.Value;
        
        if (PresencePenalty.HasValue)
            modelParameters[ModelParametersKeys.PresencePenalty] = PresencePenalty.Value;

        if (LogProbs.HasValue)
            modelParameters[ModelParametersKeys.LogProbs] = LogProbs.Value;

        if (N.HasValue)
            modelParameters[ModelParametersKeys.N] = N.Value;

        if (Seed.HasValue)
            modelParameters[ModelParametersKeys.Seed] = Seed.Value;

        if (!string.IsNullOrEmpty(Stop))
            modelParameters[ModelParametersKeys.Stop] = Stop;

        if (!string.IsNullOrEmpty(User))
            modelParameters[ModelParametersKeys.User] = User;

        if (Echo.HasValue)
            modelParameters["echo"] = Echo.Value;

        return new CompletionRequest
        {
            AgentName = Model,
            UserPrompt = Prompt,
            Settings = new OrchestrationSettings
            {
                ModelParameters = modelParameters
            }
        };
    }
} 
