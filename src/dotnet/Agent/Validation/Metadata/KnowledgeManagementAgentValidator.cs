using FluentValidation;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;

namespace FoundationaLLM.Agent.Validation.Metadata
{
    /// <summary>
    /// Validator for the <see cref="KnowledgeManagementAgent"/> model.
    /// </summary>
    public class KnowledgeManagementAgentValidator : AbstractValidator<KnowledgeManagementAgent>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="KnowledgeManagementAgent"/> model.
        /// </summary>
        public KnowledgeManagementAgentValidator()
        {
            Include(new AgentBaseValidator());

            When(a => a.Tools.Any(t => t.Category == AgentToolCategories.KnowledgeSearch), () =>
            {
                RuleFor(a => a.Tools.Where(t => t.Category == AgentToolCategories.KnowledgeSearch))
                    .Custom((tools, context) =>
                    {
                        var toolsWithDataPipeline = tools
                            .Where(t => t.TryGetResourceObjectIdsWithRole(ResourceObjectIdPropertyValues.FileUploadDataPipeline, out var result1))
                            .Select(t => t.Name)
                            .ToList();

                        if (toolsWithDataPipeline.Count > 1)
                            context.AddFailure($"At most one tool from the {AgentToolCategories.KnowledgeSearch} category is allowed to have a file upload data pipeline in its configuration.");

                        var toolsWithConversationKnowledgeUnit = tools
                            .Where(t => t.TryGetResourceObjectIdsWithRole(ResourceObjectIdPropertyValues.ConversationKnowledgeUnit, out var result2))
                            .Select(t => t.Name)
                            .ToList();

                        if (toolsWithConversationKnowledgeUnit.Count > 1)
                            context.AddFailure($"At most one tool from the {AgentToolCategories.KnowledgeSearch} category is allowed to have a conversation knowledge unit in its configuration.");

                        var toolsWithAgentPrivateStoreKnowledgeUnit = tools
                            .Where(t => t.TryGetResourceObjectIdsWithRole(ResourceObjectIdPropertyValues.AgentPrivateStoreKnowledgeUnit, out var result3))
                            .Select(t => t.Name)
                            .ToList();

                        if (toolsWithAgentPrivateStoreKnowledgeUnit.Count > 1)
                            context.AddFailure($"At most one tool from the {AgentToolCategories.KnowledgeSearch} category is allowed to have a conversation knowledge unit in its configuration.");

                        if (toolsWithDataPipeline.Count == 1
                            && toolsWithConversationKnowledgeUnit.Count == 1
                            && toolsWithAgentPrivateStoreKnowledgeUnit.Count == 1)
                        {
                            List<string> toolNames = [
                                toolsWithDataPipeline[0],
                                toolsWithConversationKnowledgeUnit[0],
                                toolsWithAgentPrivateStoreKnowledgeUnit[0]
                            ];

                            if (toolNames.Distinct().Count() > 1)
                                context.AddFailure($"The file upload data pipeline, context knowledge unit and agent private store knowledge unit must all be configured for the same tool.");
                        }
                            context.AddFailure($"The file upload data pipeline, context knowledge unit and agent private store knowledge unit must all be configured.");
                    });
            });
        }
    }
}
