using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.Attachment.Models;
using FoundationaLLM.Attachment.ResourceProviders;
using FoundationaLLM.Attachment.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Data Source resource provider service implementation of resource provider dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Add the Attachment resource provider and its related services the the dependency injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddAttachmentResourceProvider(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Attachment)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Attachment_ResourceProviderService_Storage));

            builder.Services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Attachment);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Attachment
                };
            });

            // Register validators.
            builder.Services.AddSingleton<IValidator<AttachmentFile>, AttachmentFileValidator>();

            builder.Services.AddSingleton<IResourceProviderService, AttachmentResourceProviderService>(sp =>
                new AttachmentResourceProviderService(
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IAuthorizationService>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Attachment),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp,
                    sp.GetRequiredService<ILoggerFactory>()));
            builder.Services.ActivateSingleton<IResourceProviderService>();
        }
    }
}
