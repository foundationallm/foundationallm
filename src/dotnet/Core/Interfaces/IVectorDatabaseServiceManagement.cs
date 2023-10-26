using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Core.Interfaces;

/// <summary>
/// Interface for managing vector database databases.
/// </summary>
public interface IVectorDatabaseServiceManagement
{
    /// <summary>
    /// Inserts an entity into the vector database.
    /// </summary>
    /// <param name="document">The entity to add to the vector database.</param>
    /// <returns></returns>
    Task InsertVector(object document);/// <summary>
    /// Inserts a collection of entities into the vector database.
    /// </summary>
    /// <param name="documents">The entities to add to the vector database.</param>
    /// <returns></returns>
    Task InsertVectors(IEnumerable<object> documents);
    /// <summary>
    /// Deletes an entity from the vector database.
    /// </summary>
    /// <param name="document">The entity to remove from the vector database.</param>
    /// <returns></returns>
    Task DeleteVector(object document);
}