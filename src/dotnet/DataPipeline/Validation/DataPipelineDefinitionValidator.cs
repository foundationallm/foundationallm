using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Validation.Plugins;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.DataPipeline.Validation
{
    /// <summary>
    /// Validator for the <see cref="DataPipelineDefinition"/> model.
    /// </summary>
    public class DataPipelineDefinitionValidator : AbstractValidator<DataPipelineDefinition>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="DataPipelineDefinition"/> model.
        /// </summary>
        public DataPipelineDefinitionValidator()
        {
            Include(new ResourceBaseValidator());

            //RuleFor(x => x.DataSource)
            //    .NotNull()
            //    .WithMessage("The data source is required for data pipelines.");

            //RuleFor(x => x.DataSource!.DataSourceObjectId)
            //    .NotEmpty()
            //    .WithMessage("The data source object identifier is required for data pipelines.");

            //RuleFor(x => x.DataSource)
            //    .SetValidator(new PluginArtifactValidator());

            //RuleForEach(x => x.StartingStages)
            //    .ChildRules(ValidateDataPipelineStep);

            //RuleForEach(x => x.Triggers)
            //    .SetValidator(new DataPipelineTriggerValidator());
        }

        private void ValidateDataPipelineStep(InlineValidator<DataPipelineStage> stageValidator)
        {
            stageValidator.RuleFor(x => x)
                .SetValidator(new PluginComponentValidator());

            stageValidator.When(x => x.NextStages != null && x.NextStages.Count > 0, () =>
            {
                stageValidator.RuleForEach(x => x.NextStages)
                    .ChildRules(ValidateDataPipelineStep);
            });
        }
    }
}
