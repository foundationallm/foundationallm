namespace FoundationaLLM.Common.Models.Configuration.Storage;

/// <summary>
/// Provides configuration options for a Blob Storage service.
/// </summary>
public record BlobStorageSettings
{
    /// <summary>
    /// The connection string for the blob storage.
    /// </summary>
    public required string BlobStorageConnection { get; set; }

    /// <summary>
    /// The name of the blob storage container.
    /// </summary>
    public required string BlobStorageContainer { get; set; }
}
