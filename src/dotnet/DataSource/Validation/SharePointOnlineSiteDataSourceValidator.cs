using FluentValidation;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;

namespace FoundationaLLM.DataSource.Validation
{
    /// <summary>
    /// Validator for the <see cref="SharePointOnlineSiteDataSource"/> model.
    /// </summary>
    public class SharePointOnlineSiteDataSourceValidator : DataSourceValidator<SharePointOnlineSiteDataSource>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="SharePointOnlineSiteDataSource"/> model.
        /// </summary>
        public SharePointOnlineSiteDataSourceValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            Include(new DataSourceBaseValidator());

            RuleFor(x => x.SiteUrl)
                .NotNull()
                .NotEmpty()
                .WithMessage("The site URL must be a valid string.");

            RuleFor(x => x.DocumentLibraries)
                .NotNull()
                .NotEmpty()
                .WithMessage("The list of document library paths must contain at least one value.");

            RuleForEach(x => x.DocumentLibraries)
                .NotNull()
                .NotEmpty()
                .WithMessage("Each document library path must be a valid string.");

            RuleFor(x => x)
                .Must(ds => ValidConfigurationReference(ds, "ClientId"))
                .WithMessage("The ClientId configuration reference is missing or has an invalid value.");

            RuleFor(x => x)
                .Must(ds => ValidConfigurationReference(ds, "TenantId"))
                .WithMessage("The TenantId configuration reference is missing or has an invalid value.");

            RuleFor(x => x)
                .Must(ds => ValidConfigurationReference(ds, "KeyVaultURL"))
                .WithMessage("The KeyVaultURL configuration reference is missing or has an invalid value.");

            RuleFor(x => x)
                .Must(ds => ValidConfigurationReference(ds, "CertificateName"))
                .WithMessage("The CertificateName configuration reference is missing or has an invalid value.");
        }
    }
}
