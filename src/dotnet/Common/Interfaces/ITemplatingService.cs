namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Defines the interface for a templating engine.
    /// </summary>
    public interface ITemplatingService
    {
        /// <summary>
        /// Transforms the input string by replacing tokens with the corresponding values.
        /// </summary>
        /// <param name="s">The input string to be transformed.</param>
        /// <returns>The transformed string where all the valid tokens have been replaced.</returns>
        string Transform(string s);
    }
}
