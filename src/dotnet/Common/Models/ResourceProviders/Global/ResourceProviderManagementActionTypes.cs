using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Global
{
    /// <summary>
    /// Defines the types of management actions that can be performed on resource providers.
    /// </summary>
    public static class ResourceProviderManagementActionTypes
    {
        /// <summary>
        /// Represents an action to send an event to a resource provider.
        /// </summary>
        public const string SendEvent = "send-event";
    }
}
