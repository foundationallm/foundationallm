using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.AzureAIService
{
    public class DataVersionResponse
    {
        [JsonPropertyName("dataVersion")]
        public DataVersion DataVersion { get; set; }

        [JsonPropertyName("entityMetadata")]
        public EntityMetadata EntityMetadata { get; set; }

        [JsonPropertyName("legacyDatasetId")]
        public string LegacyDatasetId { get; set; }

        [JsonPropertyName("isV2")]
        public bool IsV2 { get; set; }

        [JsonPropertyName("legacyDatasetType")]
        public object LegacyDatasetType { get; set; }

        [JsonPropertyName("legacyDataflowType")]
        public object LegacyDataflowType { get; set; }

        [JsonPropertyName("legacyDataflow")]
        public object LegacyDataflow { get; set; }

        [JsonPropertyName("legacySavedDatasetId")]
        public object LegacySavedDatasetId { get; set; }

        [JsonPropertyName("putAssetLROResponseDto")]
        public object PutAssetLROResponseDto { get; set; }
    }

    public class DataVersion
    {
        [JsonPropertyName("assetId")]
        public string AssetId { get; set; }

        [JsonPropertyName("dataContainerName")]
        public string DataContainerName { get; set; }

        [JsonPropertyName("dataType")]
        public string DataType { get; set; }

        [JsonPropertyName("dataUri")]
        public string DataUri { get; set; }

        [JsonPropertyName("versionId")]
        public string VersionId { get; set; }

        [JsonPropertyName("mutableProps")]
        public MutableProps MutableProps { get; set; }

        [JsonPropertyName("referencedDataUris")]
        public object ReferencedDataUris { get; set; }

        [JsonPropertyName("properties")]
        public object Properties { get; set; }

        [JsonPropertyName("initialAssetId")]
        public string InitialAssetId { get; set; }

        [JsonPropertyName("isRegistered")]
        public bool IsRegistered { get; set; }

        [JsonPropertyName("runId")]
        public object RunId { get; set; }

        [JsonPropertyName("originAssetId")]
        public object OriginAssetId { get; set; }
    }

    public class MutableProps
    {
        [JsonPropertyName("dataExpiryTime")]
        public object DataExpiryTime { get; set; }

        [JsonPropertyName("description")]
        public object Description { get; set; }

        [JsonPropertyName("tags")]
        public object Tags { get; set; }

        [JsonPropertyName("isArchived")]
        public bool IsArchived { get; set; }

        [JsonPropertyName("stage")]
        public string Stage { get; set; }

        [JsonPropertyName("autoDeleteSetting")]
        public object AutoDeleteSetting { get; set; }
    }

    public class EntityMetadata
    {
        [JsonPropertyName("etag")]
        public string Etag { get; set; }

        [JsonPropertyName("createdTime")]
        public DateTime CreatedTime { get; set; }

        [JsonPropertyName("modifiedTime")]
        public DateTime ModifiedTime { get; set; }

        [JsonPropertyName("createdBy")]
        public CreatedBy CreatedBy { get; set; }

        [JsonPropertyName("modifiedBy")]
        public object ModifiedBy { get; set; }
    }

    public class CreatedBy
    {
        [JsonPropertyName("userObjectId")]
        public string UserObjectId { get; set; }

        [JsonPropertyName("userPuId")]
        public string UserPuId { get; set; }

        [JsonPropertyName("userIdp")]
        public object UserIdp { get; set; }

        [JsonPropertyName("userAltSecId")]
        public object UserAltSecId { get; set; }

        [JsonPropertyName("userIss")]
        public string UserIss { get; set; }

        [JsonPropertyName("userTenantId")]
        public string UserTenantId { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("upn")]
        public string Upn { get; set; }
    }


}
