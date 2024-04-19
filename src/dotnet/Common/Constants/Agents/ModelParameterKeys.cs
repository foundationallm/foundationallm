namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Contains constants of the keys for all overridable model settings.
    /// </summary>
    public static class ModelParameterKeys
    {
        /// <summary>
        /// The key name for the deployment_name model parameter.
        /// This value should be a string representing the name of the model deployment in Azure OpenAI.
        /// </summary>
        public const string DeploymentName = "deployment_name";
        /// <summary>
        /// The key name for the model version parameter.
        /// This value should be a string representing the version of the model to use.
        /// </summary>
        public const string Version = "version";
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
        /// Lowering Top P will narrow the model’s token selection to likelier tokens. Increasing Top P will let the model
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
        public const string MaxNewTokens = "max_new_tokens";
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
    }
}
