namespace FoundationaLLM.Common.Constants.Agents
{
    /// <summary>
    /// Contains constants of the keys for all overridable Agent settings.
    /// </summary>
    public static class AgentParameterKeys
    {
        /// <summary>
        /// The key name for the index filter expression agent parameter.
        /// This value should be a string representing the search filter expression to limit documents to be searched
        /// by the index.
        /// </summary>
        public const string IndexFilterExpression = "index_filter_expression";
        /// <summary>
        /// Controls the number of search results to return from an index for prompt augmentation.
        /// </summary>
        public const string IndexTopN = "index_top_n";

    }
}
