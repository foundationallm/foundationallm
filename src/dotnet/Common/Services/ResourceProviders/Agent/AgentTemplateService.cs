using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentTemplates;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace FoundationaLLM.Common.Services.ResourceProviders.Agent
{
    /// <summary>
    /// Provides functionality for interacting with agent templates.
    /// </summary>
    public partial class AgentTemplateService : IAgentTemplateService
    {
        private readonly ILogger<AgentTemplateService> _logger;

        private IResourceProviderService _agentResourceProviderService = null!;
        private IResourceProviderService _promptResourceProviderService = null!;
        private IResourceProviderService _authorizationResourceProviderService = null!;

        private const string AGENT_FILE_NAME = "agent.json";
        private const string PROMPTS_FILE_NAME = "prompts.json";
        private const string WORKFLOW_FILE_NAME = "workflow.json";
        private const string TOOLS_FILE_NAME = "tools.json";
        private const string ROLE_ASSIGNMENTS_FILE_NAME = "roleAssignments.json";
        private const string PARAMETERS_FILE_NAME = "parameters.json";

        private readonly List<string> ALL_FILE_NAMES = [
            AGENT_FILE_NAME,
            PROMPTS_FILE_NAME,
            WORKFLOW_FILE_NAME,
            TOOLS_FILE_NAME,
            ROLE_ASSIGNMENTS_FILE_NAME,
            PARAMETERS_FILE_NAME
        ];

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTemplateService"/> class.
        /// </summary>
        /// <remarks>The constructor ensures that the service is fully initialized before any methods are
        /// called by waiting for the initialization task to complete and verifying the availability of required
        /// resource provider services.</remarks>
        /// <param name="logger">The logger instance used for logging operations within the service.</param>
        public AgentTemplateService(
            ILogger<AgentTemplateService> logger) => _logger = logger;

        /// <inheritdoc />
        public void SetResourceProviderServices(
            IEnumerable<IResourceProviderService> resourceProviderServices)
        {
            _agentResourceProviderService =
                resourceProviderServices.SingleOrDefault(x => x.Name == ResourceProviderNames.FoundationaLLM_Agent)
                ?? throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Agent} is not available.");
            _promptResourceProviderService =
                resourceProviderServices.SingleOrDefault(x => x.Name == ResourceProviderNames.FoundationaLLM_Prompt)
                ?? throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Prompt} is not available.");
            _authorizationResourceProviderService =
                resourceProviderServices.SingleOrDefault(x => x.Name == ResourceProviderNames.FoundationaLLM_Authorization)
                ?? throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Authorization} is not available.");
        }

        /// <inheritdoc />
        public async Task<ResourceProviderUpsertResult> CreateAgent(
            string instanceId,
            AgentCreationFromTemplateRequest createAgentRequest,
            Dictionary<string, string> rawAgentTemplateFiles,
            UnifiedUserIdentity userIdentity)
        {
            var agentTemplateFiles = ValidateAndDeserializeTemplateFiles(
                rawAgentTemplateFiles);
            var finalParameterValues = agentTemplateFiles[PARAMETERS_FILE_NAME]
                as Dictionary<string, string>;
            ValidateAndMergeParameterValues(
                createAgentRequest.TemplateParameters,
                finalParameterValues!);

            // Using the hosting service identity to check for existing agents to avoid
            // the need to assign additional permissions to end users.
            var (Exists, _) = await _agentResourceProviderService.ResourceExistsAsync<AgentBase>(
                instanceId,
                finalParameterValues![AgentTemplateParameterNames.AgentName],
                ServiceContext.ServiceIdentity!);

            if (Exists)
                throw new ResourceProviderException(
                    $"An agent with name {finalParameterValues[AgentTemplateParameterNames.AgentName]} already exists in the instance {instanceId}.",
                    StatusCodes.Status400BadRequest);

            var promptObjectIds = await CreatePrompts(
                instanceId,
                agentTemplateFiles,
                rawAgentTemplateFiles,
                finalParameterValues!,
                userIdentity);

            var workflowNode = TransformWorkflow(
                instanceId,
                (JsonElement)agentTemplateFiles[WORKFLOW_FILE_NAME],
                finalParameterValues!);
            var toolsArray = TransformTools(
                instanceId,
                (JsonElement)agentTemplateFiles[TOOLS_FILE_NAME],
                finalParameterValues!);

            var agentCreationResult = await CreateAgent(
                instanceId,
                (JsonElement)agentTemplateFiles[AGENT_FILE_NAME],
                finalParameterValues!,
                workflowNode,
                toolsArray,
                userIdentity);

            return agentCreationResult;
        }

        private Dictionary<string, object> ValidateAndDeserializeTemplateFiles(
            Dictionary<string, string> agentTemplateFiles)
        {
            Dictionary<string, object> result = [];

            foreach (var fileName in ALL_FILE_NAMES)
            {
                if (!agentTemplateFiles.TryGetValue(fileName, out var fileContent)
                    || string.IsNullOrWhiteSpace(fileContent))
                    throw new ResourceProviderException(
                        $"The file name {fileName} was not provided in the list agent template files.");

                result.Add(
                    fileName,
                    fileName == PARAMETERS_FILE_NAME
                        ? JsonSerializer.Deserialize<Dictionary<string, string>>(fileContent)!
                        : JsonSerializer.Deserialize<JsonElement>(fileContent));
            }

            return result;
        }

        private void ValidateAndMergeParameterValues(
            Dictionary<string, string> inputParameterValues,
            Dictionary<string, string> finalParameterValues)
        {
            var invalidParameterNames = inputParameterValues.Keys
                .Where(name =>
                    !finalParameterValues.ContainsKey(name))
                .ToList();
            if (invalidParameterNames.Count > 0)
                throw new ResourceProviderException(
                    $"The following parameters are invalid: {string.Join(',', invalidParameterNames)}");

            foreach (var parameterName in finalParameterValues.Keys)
            {
                if (inputParameterValues.TryGetValue(parameterName, out var value)
                    || !string.IsNullOrWhiteSpace(value))
                    finalParameterValues[parameterName] = value;

                if (string.IsNullOrWhiteSpace(finalParameterValues[parameterName]))
                    finalParameterValues[parameterName] = parameterName switch
                    {
                        AgentTemplateParameterNames.AgentName
                        or AgentTemplateParameterNames.AgentDisplayName
                        or AgentTemplateParameterNames.AgentTools
                        or AgentTemplateParameterNames.MainLLM
                        or AgentTemplateParameterNames.MainKnowledgeLLM =>
                            throw new ResourceProviderException(
                                $"The parameter {parameterName} is required and cannot be empty.",
                                StatusCodes.Status400BadRequest),
                        AgentTemplateParameterNames.VirtualSecurityGroupId =>
                            Guid.NewGuid().ToString().ToLower(),
                        _ => null!
                    };
            }
        }

        private async Task<Dictionary<string, string>> CreatePrompts(
            string instanceId,
            Dictionary<string, object> agentTemplateFiles,
            Dictionary<string, string> rawAgentTemplateFiles,
            Dictionary<string, string> parameterValues,
            UnifiedUserIdentity userIdentity)
        {
            var promptsFile = (JsonElement)agentTemplateFiles[PROMPTS_FILE_NAME];

            var node = JsonNode.Parse(promptsFile.GetRawText())!;

            if (node is JsonArray promptsArray)
            {
                foreach (var promptNode in promptsArray)
                    if (promptNode is JsonObject promptObject)
                    {
                        promptObject["name"] =
                            promptObject["name"]?.ToString().Replace("{{AGENT_NAME}}", parameterValues[AgentTemplateParameterNames.AgentName]);
                        promptObject["display_name"] =
                            promptObject["display_name"]?.ToString().Replace("{{AGENT_DISPLAY_NAME}}", parameterValues[AgentTemplateParameterNames.AgentDisplayName]);
                        promptObject["description"] =
                            promptObject["description"]?.ToString().Replace("{{AGENT_DISPLAY_NAME}}", parameterValues[AgentTemplateParameterNames.AgentName]);

                        var promptContentFileName = promptObject["prefix"]?.ToString() ?? string.Empty;
                        if (rawAgentTemplateFiles.TryGetValue(promptContentFileName, out var promptContent))
                            promptObject["prefix"] = promptContent;
                        else
                            throw new ResourceProviderException(
                                $"The prompt content file {promptContentFileName} was not provided in the list of agent template files.");
                    }
                    else
                        throw new ResourceProviderException(
                            $"The prompts file {PROMPTS_FILE_NAME} has an invalid structure.");

                var promptsToCreate = promptsArray
                    .Select(promptNode => JsonSerializer.Deserialize<PromptBase>(promptNode!.ToJsonString())!)
                    .ToList();

                var promptCreationTasks = promptsToCreate
                    .Select(prompt => _promptResourceProviderService.UpsertResourceAsync<PromptBase, ResourceProviderUpsertResult<PromptBase>>(
                        instanceId,
                        prompt,
                        userIdentity))
                    .ToList();

                var promptCreationResults = await Task.WhenAll(promptCreationTasks);

                return promptsToCreate
                    .Zip(promptCreationResults)
                    .ToDictionary(
                        x => x.First.Name,
                        x => x.Second.ObjectId);
            }
            else
                throw new ResourceProviderException(
                    $"The prompts file {PROMPTS_FILE_NAME} has an invalid structure.");
        }

        private async Task<ResourceProviderUpsertResult<AgentBase>> CreateAgent(
            string instanceId,
            JsonElement agentElement,
            Dictionary<string, string> parameterValues,
            JsonNode workflowNode,
            JsonArray toolsArray,
            UnifiedUserIdentity userIdentity)
        {
            var node = JsonNode.Parse(agentElement.GetRawText())!;
            if (node is JsonObject agentObject)
            {
                ReplaceParameterValues(
                    agentObject,
                    parameterValues);

                agentObject["workflow"] = workflowNode;
                agentObject["tools"] = toolsArray;

                var agent = JsonSerializer.Deserialize<GenericAgent>(
                    agentObject.ToJsonString());

                agent!.OwnerUserId = userIdentity.UserId;

                var agentCreationResult = await _agentResourceProviderService.UpsertResourceAsync<AgentBase, ResourceProviderUpsertResult<AgentBase>>(
                    instanceId,
                    (agent as AgentBase)!,
                    userIdentity);

                return
                    agentCreationResult;
            }
            else
                throw new ResourceProviderException(
                    $"The agent file {AGENT_FILE_NAME} has an invalid structure.");
        }

        private JsonNode TransformWorkflow(
            string instanceId,
            JsonElement workflowElement,
            Dictionary<string, string> parameterValues)
        {
            var node = JsonNode.Parse(workflowElement.GetRawText())!;
            if (node is JsonObject workflowObject)
            {
                if (workflowObject.ContainsKey("resources")
                    && workflowObject["resources"] is JsonArray resourcesArray)
                {
                    workflowObject["resource_object_ids"] = TransformResourcesArray(
                        instanceId,
                        resourcesArray,
                        parameterValues);
                    workflowObject.Remove("resources");
                }
            }
            else
                throw new ResourceProviderException(
                    $"The workflow file {WORKFLOW_FILE_NAME} has an invalid structure.");

            return node;
        }

        private JsonArray TransformTools(
            string instanceId,
            JsonElement toolsElement,
            Dictionary<string, string> parameterValues)
        {
            var node = JsonNode.Parse(toolsElement.GetRawText())!;
            if (node is JsonArray toolsArray)
            {
                foreach (var toolNode in toolsArray)
                {
                    if (toolNode is JsonObject toolObject)
                    {
                        if (toolObject.ContainsKey("resources")
                            && toolObject["resources"] is JsonArray resourcesArray)
                        {
                            toolObject["resource_object_ids"] = TransformResourcesArray(
                                instanceId,
                                resourcesArray,
                                parameterValues);
                            toolObject.Remove("resources");
                        }

                        if (toolObject.ContainsKey("properties")
                            && toolObject["properties"] is JsonObject propertiesObject)
                            ReplaceParameterValues(
                                propertiesObject,
                                parameterValues);
                    }
                    else
                        throw new ResourceProviderException(
                            $"The tools file {TOOLS_FILE_NAME} has an invalid structure.");
                }

                // Only keep the tools that are specified in the agent tools list.
                var toolsList = parameterValues[AgentTemplateParameterNames.AgentTools]
                   .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                // Use indexing to avoid issues when removing during enumeration
                for (int i = toolsArray.Count - 1; i >= 0; i--)
                    if (toolsArray[i] is JsonObject toolObject
                        && !toolsList.Contains(toolObject["name"]!.ToString()))
                        toolsArray.RemoveAt(i);

                return toolsArray;
            }
            else
                throw new ResourceProviderException(
                    $"The tools file {TOOLS_FILE_NAME} has an invalid structure.");
        }

        private JsonObject TransformResourcesArray(
            string instanceId,
            JsonArray resourcesArray,
            Dictionary<string, string> parameterValues)
        {
            var result = new JsonObject();
            foreach (var resourceNode in resourcesArray)
            {
                if (resourceNode is JsonObject resourceObject)
                {
                    var objectId = GetResourceObjectId(
                        instanceId,
                        resourceObject,
                        parameterValues);

                    var newProperties = new JsonObject();
                    var newResourceObjectId = new JsonObject
                    {
                        ["object_id"] = objectId,
                        ["properties"] = newProperties
                    };

                    if (resourceObject.ContainsKey("role"))
                        newProperties["object_role"] =
                            resourceObject["role"]?.ToString();

                    if (resourceObject.ContainsKey("properties"))
                        if (resourceObject["properties"] is JsonArray propertiesArrayArray)
                        {
                            foreach (var propertiesArrayElement in propertiesArrayArray)
                                if (propertiesArrayElement is JsonArray propertiesArray)
                                {
                                    if (propertiesArray.Count != 3
                                        || propertiesArray[0] is null
                                        || propertiesArray[0] is not JsonValue
                                        || propertiesArray[0]!.GetValueKind() != JsonValueKind.String
                                        || propertiesArray[1] is null
                                        || propertiesArray[1] is not JsonValue
                                        || propertiesArray[1]!.GetValueKind() != JsonValueKind.String
                                        || propertiesArray[2] is null
                                        || propertiesArray[2] is not JsonValue
                                        || (propertiesArray[2]!.GetValueKind() != JsonValueKind.String && propertiesArray[2]!.GetValueKind() != JsonValueKind.Number))
                                        throw new ResourceProviderException(
                                            $"The resource object {resourceObject["name"]} has an invalid 'properties' structure. Each element of the array should be an array with three elements: [string, string, string or number].");

                                    if (newProperties.ContainsKey(propertiesArray[0]!.ToString()!))
                                        newProperties[propertiesArray[0]!.ToString()!]![propertiesArray[1]!.ToString()!] =
                                            propertiesArray[2]!.DeepClone();
                                    else
                                        newProperties[propertiesArray[0]!.ToString()!] = new JsonObject
                                        {
                                            [propertiesArray[1]!.ToString()!] = propertiesArray[2]!.DeepClone()
                                        };
                                }
                                else
                                    throw new ResourceProviderException(
                                        $"The resource object {resourceObject["name"]} has an invalid 'properties' structure. It should be an array of elements.");
                        }
                        else
                            throw new ResourceProviderException(
                                $"The resource object {resourceObject["name"]} has an invalid 'properties' structure. It should be an array.");

                    result.Add(objectId, newResourceObjectId);
                }
                else
                    throw new ResourceProviderException(
                        $"The resources array has an invalid structure.");
            }
            return result;
        }

        private string GetResourceObjectId(
            string instanceId,
            JsonObject jsonObject,
            Dictionary<string, string> parameterValues)
        {
            if (!jsonObject.ContainsKey("name")
                || !jsonObject.ContainsKey("type"))
                throw new ResourceProviderException(
                    "The JSON object is missing either one of 'name' or 'type' properties and cannot be used to derive a resource object identifier.");

            var name = ReplaceParameterValues(
                jsonObject["name"]?.ToString(),
                parameterValues)
                ?? throw new ResourceProviderException(
                    "The JSON object has an invalid value for the 'name' property and cannot be used to derive a resource object identifier.");
            var type = jsonObject["type"]?.ToString()
                ?? throw new ResourceProviderException(
                    "The JSON object has an invalid value for the 'type' property and cannot be used to derive a resource object identifier.");

            return $"/instances/{instanceId}/providers/{type}/{name}";
        }

        private void ReplaceParameterValues(
            JsonObject jsonObject,
            Dictionary<string, string> parameterValues)
        {
            // Use indexing to avoid issues when modifying the collection during traversal
            var propertyNames = jsonObject.Select(p => p.Key).ToList();
            foreach (var propertyName in propertyNames)
            {
                var value = jsonObject[propertyName];
                if (value is JsonObject childObject)
                {
                    ReplaceParameterValues(childObject, parameterValues);
                }
                else if (value is JsonArray array)
                {
                    ReplaceParameterValues(array, parameterValues);
                }
                else if (value is JsonValue val && val.TryGetValue<string>(out var strValue))
                {
                    jsonObject[propertyName] = ReplaceParameterValues(strValue, parameterValues);
                }
            }
        }

        private void ReplaceParameterValues(
            JsonArray jsonArray,
            Dictionary<string, string> parameterValues)
        {
            for (int i = 0; i < jsonArray.Count; i++)
            {
                var item = jsonArray[i];
                if (item is JsonObject childObject)
                {
                    ReplaceParameterValues(childObject, parameterValues);
                }
                else if (item is JsonArray childArray)
                {
                    ReplaceParameterValues(childArray, parameterValues);
                }
                else if (item is JsonValue value && value.TryGetValue<string>(out var strValue))
                {
                    jsonArray[i] = ReplaceParameterValues(strValue, parameterValues);
                }
            }
        }

        private string? ReplaceParameterValues(
            string? value,
            Dictionary<string, string> parameterValues)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            // Try to match the entire value first for efficiency.
            var matches = VariableRegex().Matches(value);
            if (matches.Count == 1 && matches[0].Groups.Count == 2)
            {
                var parameterName = matches[0].Groups[1].Value;
                if (parameterValues.TryGetValue(parameterName, out var parameterValue))
                    return parameterValue;
            }

            // The parameter might be part of a larger string, so replace all occurrences.
            foreach (var parameter in parameterValues)
            {
                value = value.Replace("{{" + parameter.Key + "}}", parameter.Value);
            }
            return value;
        }

        [GeneratedRegex(@"^\{\{([^}]+)\}\}$")]
        private static partial Regex VariableRegex();
    }
}
