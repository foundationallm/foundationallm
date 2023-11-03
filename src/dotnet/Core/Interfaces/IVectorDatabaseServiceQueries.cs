using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Core.Interfaces;

/// <summary>
/// Interface for the vector database service search queries.
/// </summary>
public interface IVectorDatabaseServiceQueries
{
    /// <summary>
    /// Performs a vector similarity search against the vector database.
    /// </summary>
    /// <param name="embeddings">The vector used in the index search.</param>
    /// <returns></returns>
    Task<string> VectorSearchAsync(float[] embeddings);
}