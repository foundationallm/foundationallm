using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;

namespace FoundationaLLM.DataPipeline.Validation
{
    /// <summary>
    /// Validator for the <see cref="DataPipelineTrigger"/> model.
    /// </summary>
    public class DataPipelineTriggerValidator: AbstractValidator<DataPipelineTrigger>
    {
        public DataPipelineTriggerValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("The trigger name is required for data pipeline triggers.");

            When(x => x.TriggerType == DataPipelineTriggerType.Schedule, () =>
            {
                RuleFor(x => x.TriggerCronSchedule)
                    .NotEmpty()
                    .WithMessage("The schedule is required for scheduled data pipeline triggers.");
            });

            //TODO: Add validation for the parameter values.
        }
    }
}
