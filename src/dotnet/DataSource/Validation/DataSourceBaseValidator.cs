using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Validation.ResourceProvider;

namespace FoundationaLLM.DataSource.Validation
{
    /// <summary>
    /// Validator for the <see cref="DataSourceBase"/> model.
    /// </summary>
    public class DataSourceBaseValidator : AbstractValidator<DataSourceBase>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="DataSourceBase"/> model.
        /// </summary>
        public DataSourceBaseValidator() =>
            Include(new ResourceBaseValidator());
    }
}
