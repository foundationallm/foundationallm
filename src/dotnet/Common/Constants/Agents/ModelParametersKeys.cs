using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Contains constants of the keys for all overridable model settings.
    /// </summary>
    public static class ModelParametersKeys
    {
        /// <summary>
        /// Generates best_of completions server-side and returns the "best" (the one with the highest log probability per token).
        /// Results can't be streamed.
        /// 
        /// When used with n, best_of controls the number of candidate completions and n specifies how many to return.
        /// best_of must be greater than n.
        /// </summary>
        public const string BestOf = "best_of";

        /// <summary>
        /// Echo back the prompt in addition to the completion.
        /// </summary>
        public const string Echo = "echo";

        /// <summary>
        /// Return a list of the specified number of most likely tokens sorted by their logprobs.
        /// </summary>
        public const string LogProbs = "logprobs";

        /// <summary>
        /// The number of completions to generate for each prompt.
        /// </summary>
        public const string N = "n";

        /// <summary>
        /// If specified, the system will make a best effort to sample deterministically,
        /// such that repeated requests with the same seed and parameters should return
        /// the same result.
        /// </summary>
        public const string Seed = "seed";

        /// <summary>
        /// Sequence where the API will stop generating further tokens.
        /// </summary>
        public const string Stop = "stop";

        /// <summary>
        /// Controls randomness. Lowering the temperature means that the model will produce more repetitive and
        /// deterministic responses. Increasing the temperature will result in more unexpected or creative responses.
        /// Try adjusting temperature or Top P but not both. This value should be a float between 0.0 and 1.0.
        /// </summary>
        public const string Temperature = "temperature";

        /// <summary>
        /// The number of highest probability vocabulary tokens to keep for top-k-filtering.
        /// Default value is null, which disables top-k-filtering.
        /// </summary>
        public const string TopK = "top_k";

        /// <summary>
        /// The cumulative probability of parameter highest probability vocabulary tokens to keep for nucleus sampling.
        /// Top P (or Top Probabilities) is imilar to temperature, this controls randomness but uses a different method.
        /// Lowering Top P will narrow the model's token selection to likelier tokens. Increasing Top P will let the model
        /// choose from tokens with both high and low likelihood. Try adjusting temperature or Top P but not both.
        /// </summary>
        public const string TopP = "top_p";

        /// <summary>
        /// Whether or not to use sampling; use greedy decoding otherwise.
        /// </summary>
        public const string DoSample = "do_sample";

        /// <summary>
        /// Sets a limit on the number of tokens per model response. The API supports a maximum of 4000 tokens shared
        /// between the prompt (including system message, examples, message history, and user query) and the model's
        /// response. One token is roughly 4 characters for typical English text.
        /// </summary>
        public const string MaxTokens = "max_tokens";

        /// <summary>
        /// Whether or not to return the full text (prompt + response) or only the generated part (response).
        /// Default value is false.
        /// </summary>
        public const string ReturnFullText = "return_full_text";

        /// <summary>
        /// Whether to ignore the EOS token and continue generating tokens after the EOS token is generated.
        /// Defaults to False.
        /// </summary>
        public const string IgnoreEOS = "ignore_eos";

        /// <summary>
        /// A unique identifier representing the end-user, which can help to monitor and detect abuse.
        /// </summary>
        public const string User = "user";

        /// <summary>
        /// Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing
        /// frequency in the text so far, decreasing the model's likelihood to repeat the same line verbatim.
        /// </summary>
        public const string FrequencyPenalty = "frequency_penalty";

        /// <summary>
        /// Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they
        /// appear in the text so far, increasing the model's likelihood to talk about new topics.
        /// </summary>
        public const string PresencePenalty = "presence_penalty";

        /// <summary>
        /// All model parameter keys.
        /// </summary>
        public readonly static string[] All = [
            Temperature,
            TopK,
            TopP,
            DoSample,
            MaxTokens,
            ReturnFullText,
            IgnoreEOS,
            FrequencyPenalty,
            PresencePenalty,
            BestOf,
            User,
            Stop,
            Seed,
            N,
            LogProbs,
            Echo
        ];

    }
}
