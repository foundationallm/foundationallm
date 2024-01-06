namespace FoundationaLLM.Common.Constants
{
    /// <summary>
    /// Core FoundationaLLM vectorization step names.
    /// </summary>
    public class VectorizationSteps
    {
        /// <summary>
        /// Extract the text from the content of the document.
        /// </summary>
        public const string Extract = "extract";

        /// <summary>
        /// Partition the text into multiple chunks.
        /// </summary>
        public const string Partition = "partition";

        /// <summary>
        /// Embed text chunks into a latent vector space.
        /// </summary>
        public const string Embed = "embed";

        /// <summary>
        /// Persist vector embeddings into a vector index.
        /// </summary>
        public const string Index = "index";

        /// <summary>
        /// Validates a given vectorization step name.
        /// </summary>
        /// <param name="stepName">The vectorization step name to be validated.</param>
        /// <exception cref="ArgumentException"></exception>
        public static bool ValidateStepName(string stepName) =>
            Extract.CompareTo(stepName) == 0
            || Partition.CompareTo(stepName) == 0
            || Embed.CompareTo(stepName) == 0
            || Index.CompareTo(stepName) == 0;
    }
}
