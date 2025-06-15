namespace FoundationaLLM.Common.Constants.Events
{
    /// <summary>
    /// Provide event type constants.
    /// </summary>
    public static class EventTypes
    {
        /// <summary>
        /// The resource provider cache reset command event type.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_Cache_ResetCommand = "FoundationaLLM.ResourceProvider.Cache.ResetCommand";

        /// <summary>
        /// The resource provider state export command event type.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_State_ExportCommand = "FoundationaLLM.ResourceProvider.State.ExportCommand";

        /// <summary>
        /// The FoundationaLLM.Configuration update key command event type.
        /// </summary>
        public const string FoundationaLLM_ResourceProvider_AppConfig_UpdateKeyCommand = "FoundationaLLM.ResourceProvider.AppConfig.UpdateKeyCommand";

        /// <summary>
        /// The FoundationaLLM quots metric update event type.
        /// </summary>
        public const string FoundationaLLM_Quota_MetricUpdate = "FoundationaLLM.Quota.MetricUpdate";

        /// <summary>
        /// All event types.
        /// </summary>
        public static List<string> All =>
            [
                FoundationaLLM_ResourceProvider_Cache_ResetCommand,
                FoundationaLLM_ResourceProvider_State_ExportCommand,
                FoundationaLLM_ResourceProvider_AppConfig_UpdateKeyCommand,
                FoundationaLLM_Quota_MetricUpdate
            ];
    }
}
