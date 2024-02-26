using FluentValidation;
using FoundationaLLM.Vectorization.Models.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Vectorization.Validation.Resources
{
    /// <summary>
    /// Validator for the <see cref="ContentSourceProfile"/> model.
    /// </summary>
    public class ContentSourceProfileValidator : AbstractValidator<ContentSourceProfile>
    {
        /// <summary>
        /// Configures the validation rules for the <see cref="ContentSourceProfile"/> model.
        /// </summary>
        public ContentSourceProfileValidator()
        {
            // Include the base validator to apply its rules
            Include(new VectorizationProfileBaseValidator());

            // Validate the Type property to ensure it's a valid enum value.
            RuleFor(x => x.ContentSource)
                .IsInEnum().WithMessage("The content source type must be a valid value.");

            RuleFor(profile => profile.ConfigurationReferences)
                .Must((profile, configurationReferences) =>
                {
                    if (configurationReferences == null || !configurationReferences.Any())
                    {
                        return true;
                    }

                    var keysToCheck = new[] { "AuthenticationType", "ConnectionString", "AccountName" };

                    foreach (var key in keysToCheck)
                    {
                        if (configurationReferences.TryGetValue(key, out var value))
                        {
                            if (!value.Contains(profile.Name))
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                })
                .WithMessage("ConfigurationReferences must include the profile Name in values for 'AuthenticationType', 'ConnectionString', or 'AccountName'.");
        }
    }
}
