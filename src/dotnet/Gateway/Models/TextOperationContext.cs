using FoundationaLLM.Common.Models.Vectorization;
using OpenAI.Chat;

namespace FoundationaLLM.Gateway.Models
{
    /// <summary>
    /// Provides the context for a text operation.
    /// </summary>
    public class TextOperationContext
    {
        protected readonly object _syncRoot = new();

        /// <summary>
        /// The action used to update internal text chunks based on incoming text chunks.
        /// </summary>
        public Action<TextChunk, TextChunk> TextChunkUpdater { get; set; } = null!;

        /// <summary>
        /// The function used to check if a text chunk is complete.
        /// </summary>
        public Func<TextChunk, bool> TextChunkCompletenessChecker { get; set; } = null!;

        /// <summary>
        /// The position-indexed dictionary of <see cref="TextChunk"/> objects which provide the input to the text operation.
        /// </summary>
        public required Dictionary<int, TextChunk> InputTextChunks { get; set; } = [];

        /// <summary>
        /// The <see cref="TextOperationResult"/> holding the result of the text operation.
        /// </summary>
        public required TextOperationResult Result { get; set; }

        /// <summary>
        /// Indicates whether the requests associated with the context should be prioritized.
        /// Example: Synchronous vectorization and user prompt embedding for completions.
        /// </summary>        
        public bool Prioritized { get; set; } = false;

        /// <summary>
        /// Gets or sets the list of intermediate error messages encountered during the text operation.
        /// </summary>
        public List<string> IntermediateErrors { get; set; } = [];

        /// <summary>
        /// Gets or sets the model parameters.
        /// </summary>
        public Dictionary<string, object> ModelParameters { get; set; } = [];

        /// <summary>
        /// Sets a specified error message on the context of the text operation.
        /// </summary>
        /// <param name="errorMessage">The error message to be set.</param>
        public void SetIntermediateError(string errorMessage)
        {
            lock (_syncRoot)
            {
                IntermediateErrors.Add(errorMessage);

                if (IntermediateErrors.Count >= 3)
                {
                    Result.ErrorMessage = string.Join(string.Empty,
                        [
                            $"The text operation {Result.OperationId} encountered {IntermediateErrors.Count} errors and failed.",
                            $"The most recent error message was: {errorMessage}"
                        ]);
                    Result.Failed = true;
                    Result.InProgress = false;
                }
            }
        }

        /// <summary>
        /// Updates the text chunks using the specified update and completeness check lambdas. 
        /// If all positions are complete, marks the operation as complete.
        /// </summary>
        /// <param name="textChunks">A list of <see cref="TextChunk"/> objects containing positions and their associated values (embeddings or completions).</param>
        public void UpdateTextChunks(
            IList<TextChunk> textChunks)
        {
            lock(_syncRoot)
            {
                foreach (var textChunk in  textChunks)
                {
                    TextChunkUpdater(
                        Result.TextChunks.Single(resultTextChunk => resultTextChunk.Position == textChunk.Position),
                        textChunk);
                }

                Result.ProcessedTextChunksCount =
                    Result.TextChunks.Count(tc => TextChunkCompletenessChecker(tc));
                if (Result.ProcessedTextChunksCount == Result.TextChunks.Count)
                    Result.InProgress = false;
            }
        }

        /// <summary>
        /// Gets the count of processed text chunks based on the completeness checker function.
        /// </summary>
        public int ProcessedTextChunksCount
        {
            get
            {
                lock (_syncRoot)
                {
                    return Result.TextChunks.Count(tc => TextChunkCompletenessChecker(tc));
                }
            }
        }
    }
}
