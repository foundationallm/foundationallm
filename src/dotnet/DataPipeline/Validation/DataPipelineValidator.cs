using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.DataPipeline.Validation
{
    public class DataPipelineValidator : AbstractValidator<DataPipelineDefinition>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="DataPipelineDefinition"/> model.
        /// </summary>
        public DataPipelineValidator() => Include(new ResourceBaseValidator());
    }
}
