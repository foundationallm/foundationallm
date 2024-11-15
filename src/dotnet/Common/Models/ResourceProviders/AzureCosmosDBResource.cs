﻿using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Extensions;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Basic model for resources persisted in Azure Cosmos DB.
    /// </summary>
    public class AzureCosmosDBResource : ResourceBase
    {
        /// <summary>
        /// The unique identifier of the conversation mapping.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// The user principal name (UPN) of the user who created the conversation mapping.
        /// </summary>
        public required string UPN { get; set; }

        /// <summary>
        /// The FoundationaLLM instance identifier
        /// </summary>
        public required string InstanceId { get; set; }
    }
}
