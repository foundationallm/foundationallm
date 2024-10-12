namespace FoundationaLLM.Common.Models.Azure.CosmosDB
{
    /// <summary>
    /// Contains the contents of an Azure Cosmos DB patch operation item.
    /// </summary>
    /// <typeparam name="T">The type of the item being updated by the patch operation.</typeparam>
    public class PatchOperationItem<T> : IPatchOperationItem
    {
        /// <inheritdoc/>
        public required string ItemId { get; set; }

        /// <inheritdoc/>
        public required Dictionary<string, object?> PropertyValues { get; set; }

        /// <inheritdoc/>
        public Type ItemType { get; } = typeof(T);
    }

    /// <summary>
    /// Represents an item that can be updated by a patch operation.
    /// </summary>
    public interface IPatchOperationItem
    {
        /// <summary>
        /// The identifier of the item being updated.
        /// </summary>
        string ItemId { get; }

        /// <summary>
        /// The dictionary containing property names and updated values.
        /// </summary>
        Dictionary<string, object?> PropertyValues { get; }

        /// <summary>
        /// Specifies the item type to aid with casting within the batch operation.
        /// </summary>
        Type ItemType { get; }
    }
}
