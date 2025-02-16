﻿using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.Attachment
{
    /// <summary>
    /// Contains a reference to an attachment.
    /// </summary>
    public class AttachmentReference : FileReference
    {
        /// <summary>
        /// Indicates an optional secondary provider for the attachment.
        /// </summary>
        /// <remarks>
        /// The only secondary provider currently supported is the FoundationaLLM.AzureOpenAI provider.
        /// </remarks>
        public string? SecondaryProvider { get; set; }

        /// <summary>
        /// Gets or sets the object identifier of the secondary provider of the attachment.
        /// </summary>
        /// <remarks>
        /// The only secondary provider currently supported is FoundationaLLM.AzureOpenAI.
        /// In this case, the object identifier is the Azure OpenAI file identifier.
        /// </remarks>
        public string? SecondaryProviderObjectId { get; set; }

        /// <summary>
        /// The object type of the resource.
        /// </summary>
        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public override Type ResourceType =>
            Type switch
            {
                AttachmentTypes.File => typeof(AttachmentFile),
                _ => throw new ResourceProviderException($"The resource type {Type} is not supported.")
            };
    }
}
