﻿using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// The result of a resource name check.
    /// </summary>
    public class ResourceNameCheckResult : ResourceName
    {
        /// <summary>
        /// The <see cref="NameCheckResultType"/> indicating whether the name is allowed or not.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public NameCheckResultType Status { get; set; }

        /// <summary>
        /// An optional message indicating why is the name not allowed.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the resource exists or not.
        /// </summary>
        /// <remarks>
        /// For logically deleted resources, the value of this property will be <c>true</c>.
        /// The <see cref="Deleted"/> property indicates whether the resource was logically deleted or not.
        /// </remarks>
        public required bool Exists { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the resource is logically deleted or not.
        /// </summary>
        public required bool Deleted { get; set; }
    }

    /// <summary>
    /// The result types of resource name checks.
    /// </summary>
    public enum NameCheckResultType
    {
        /// <summary>
        /// The name is valid and is allowed.
        /// </summary>
        Allowed,
        /// <summary>
        /// The name is invalid and cannot be used
        /// </summary>
        Denied
    }
}
