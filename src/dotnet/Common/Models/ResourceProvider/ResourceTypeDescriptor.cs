﻿namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Provides details about a resource type managed by a resource provider.
    /// </summary>
    /// <param name="resourceType">The name of the resource type.</param>
    public class ResourceTypeDescriptor(
        string resourceType)
    {
        /// <summary>
        /// The name of the resource type.
        /// </summary>
        public string ResourceType { get; set; } = resourceType;

        /// <summary>
        /// The list of actions supported by the resource type.
        /// </summary>
        public List<ResourceTypeAction> Actions { get; set; } = [];

        /// <summary>
        /// The list of <see cref="ResourceTypeAllowedTypes"/> specifying which body and return types are allowed for various HTTP methods.
        /// </summary>
        public List<ResourceTypeAllowedTypes> AllowedTypes { get; set; } = [];

        /// <summary>
        /// The dictionary of resource descriptors specifying the resource's allowed subtypes.
        /// </summary>
        public Dictionary<string, ResourceTypeDescriptor> SubTypes { get; set; } = [];
    }

    /// <summary>
    /// Provides details about a resource type action.
    /// </summary>
    /// <param name="Name">The name of the action.</param>
    /// <param name="AllowedOnResource">Indicates whether the action can be executed on individual resources.</param>
    /// <param name="AllowedOnResourceType">Indicates whether the action can be executed on the resource type itself.</param>
    /// <param name="AllowedTypes">The list of <see cref="ResourceTypeAllowedTypes"/> specifying which body and return types are allowed for various HTTP methods.</param>
    public record ResourceTypeAction(
        string Name,
        bool AllowedOnResource,
        bool AllowedOnResourceType,
        List<ResourceTypeAllowedTypes> AllowedTypes)
    {
    }

    /// <summary>
    /// Provides details about the types that are allowed for the body and return of a specific HTTP method.
    /// </summary>
    /// <param name="HttpMethod">The name of the HTTP method. Can be one of GET, POST, PUT, PATCH, or DELETE.</param>
    /// <param name="AllowedParameterTypes">The dictionary of query parameter names and types that are allowed for the method.</param>
    /// <param name="AllowedBodyTypes">The list of types that are allowed as payloads for the HTTP request.</param>
    /// <param name="AllowedReturnTypes">The list of types the are allowed as return types for the HTTP request.</param>
    public record ResourceTypeAllowedTypes(
        string HttpMethod,
        Dictionary<string, Type> AllowedParameterTypes,
        List<Type> AllowedBodyTypes,
        List<Type> AllowedReturnTypes);
}
