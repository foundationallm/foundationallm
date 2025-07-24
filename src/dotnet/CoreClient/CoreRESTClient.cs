using Azure.Core;
using FoundationaLLM.Client.Core.Clients.RESTClients;
using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using System.ClientModel;

namespace FoundationaLLM.Client.Core
{
    /// <inheritdoc/>
    public class CoreRESTClient : ICoreRESTClient
    {
        private readonly string _coreUri;
        private readonly string _instanceId;
        private readonly TokenCredential _tokenCredential;
        private readonly ApiKeyCredential _apiKeyCredential;
        private readonly APIClientSettings _options;

        /// <summary>
        /// Constructor for mocking. This does not initialize the clients.
        /// </summary>
        public CoreRESTClient()
        {
            _coreUri = null!;
            _instanceId = null!;
            _options = null!;
            _tokenCredential = null!;
            _apiKeyCredential = null!;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRESTClient"/> class and
        /// configures <see cref="IHttpClientFactory"/> with a named instance for the
        /// CoreAPI (<see cref="HttpClientNames.CoreAPI"/>) based on the passed in URL.
        /// </summary>
        /// <param name="coreUri">The base URI of the Core API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        /// <param name="instanceId">The unique (GUID) ID for the FoundationaLLM deployment.
        /// Locate this value in the FoundationaLLM Management Portal or in Azure App Config
        /// (FoundationaLLM:Instance:Id key)</param>
        public CoreRESTClient(
            string coreUri,
            TokenCredential credential,
            string instanceId)
            : this(coreUri, credential, instanceId, new APIClientSettings()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRESTClient"/> class and
        /// configures <see cref="IHttpClientFactory"/> with a named instance for the
        /// CoreAPI (<see cref="HttpClientNames.CoreAPI"/>) based on the passed in URL
        /// and optional client settings.
        /// </summary>
        /// <param name="coreUri">The base URI of the Core API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        /// <param name="instanceId">The unique (GUID) ID for the FoundationaLLM deployment.
        /// Locate this value in the FoundationaLLM Management Portal or in Azure App Config
        /// (FoundationaLLM:Instance:Id key)</param>
        /// <param name="options">Additional options to configure the HTTP Client.</param>
        public CoreRESTClient(
            string coreUri,
            TokenCredential credential,
            string instanceId,
            APIClientSettings options)
        {
            _coreUri = coreUri ?? throw new ArgumentNullException(nameof(coreUri));
            _tokenCredential = credential ?? throw new ArgumentNullException(nameof(credential));
            _apiKeyCredential = null!;
            _instanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            InitializeClients();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRESTClient"/> class and
        /// configures <see cref="IHttpClientFactory"/> with a named instance for the
        /// CoreAPI (<see cref="HttpClientNames.CoreAPI"/>) based on the passed in URL.
        /// </summary>
        /// <param name="coreUri">The base URI of the Core API.</param>
        /// <param name="credential">An <see cref="ApiKeyCredential"/> containing a valid
        /// agent access token.</param>
        /// <param name="instanceId">The unique (GUID) ID for the FoundationaLLM deployment.
        /// Locate this value in the FoundationaLLM Management Portal or in Azure App Config
        /// (FoundationaLLM:Instance:Id key)</param>
        public CoreRESTClient(
            string coreUri,
            ApiKeyCredential credential,
            string instanceId)
            : this(coreUri, credential, instanceId, new APIClientSettings()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRESTClient"/> class and
        /// configures <see cref="IHttpClientFactory"/> with a named instance for the
        /// CoreAPI (<see cref="HttpClientNames.CoreAPI"/>) based on the passed in URL
        /// and optional client settings.
        /// </summary>
        /// <param name="coreUri">The base URI of the Core API.</param>
        /// <param name="credential">An <see cref="ApiKeyCredential"/> containing a valid
        /// agent access token.</param>
        /// <param name="instanceId">The unique (GUID) ID for the FoundationaLLM deployment.
        /// Locate this value in the FoundationaLLM Management Portal or in Azure App Config
        /// (FoundationaLLM:Instance:Id key)</param>
        /// <param name="options">Additional options to configure the HTTP Client.</param>
        public CoreRESTClient(
            string coreUri,
            ApiKeyCredential credential,
            string instanceId,
            APIClientSettings options)
        {
            _coreUri = coreUri ?? throw new ArgumentNullException(nameof(coreUri));
            _tokenCredential = null!;
            _apiKeyCredential = credential ?? throw new ArgumentNullException(nameof(credential));
            _instanceId = instanceId ?? throw new ArgumentNullException(nameof(instanceId));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            InitializeClients();
        }

        /// <inheritdoc/>
        public ISessionRESTClient Sessions { get; private set; } = null!;
        /// <inheritdoc/>
        public IAttachmentRESTClient Attachments { get; private set; } = null!;
        /// <inheritdoc/>
        public IBrandingRESTClient Branding { get; private set; } = null!;
        /// <inheritdoc/>
        public ICompletionRESTClient Completions { get; private set; } = null!;
        /// <inheritdoc/>
        public IStatusRESTClient Status { get; private set; } = null!;
        /// <inheritdoc/>
        public IUserProfileRESTClient UserProfiles { get; private set; } = null!;

        private static void ConfigureHttpClient(IServiceCollection services, string coreUri, APIClientSettings options) =>
            services.AddHttpClient(HttpClientNames.CoreAPI, client =>
            {
              client.BaseAddress = new Uri(coreUri);
              client.Timeout = options.Timeout ?? TimeSpan.FromSeconds(900);
            }).AddResilienceHandler("DownstreamPipeline", static strategyBuilder =>
            {
                CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();
            });

        private void InitializeClients()
        {
            var services = new ServiceCollection();
            ConfigureHttpClient(services, _coreUri, _options);

            var serviceProvider = services.BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();

            if (_tokenCredential is not null)
            {
                Sessions = new SessionRESTClient(httpClientFactory, _tokenCredential, _instanceId);
                Attachments = new AttachmentRESTClient(httpClientFactory, _tokenCredential, _instanceId);
                Branding = new BrandingRESTClient(httpClientFactory, _tokenCredential, _instanceId);
                Completions = new CompletionRESTClient(httpClientFactory, _tokenCredential, _instanceId);
                Status = new StatusRESTClient(httpClientFactory, _tokenCredential, _instanceId);
                UserProfiles = new UserProfileRESTClient(httpClientFactory, _tokenCredential, _instanceId);
            }
            else if (_apiKeyCredential is not null)
            {
                Sessions = new SessionRESTClient(httpClientFactory, _apiKeyCredential, _instanceId);
                Attachments = new AttachmentRESTClient(httpClientFactory, _apiKeyCredential, _instanceId);
                Branding = new BrandingRESTClient(httpClientFactory, _apiKeyCredential, _instanceId);
                Completions = new CompletionRESTClient(httpClientFactory, _apiKeyCredential, _instanceId);
                Status = new StatusRESTClient(httpClientFactory, _apiKeyCredential, _instanceId);
                UserProfiles = new UserProfileRESTClient(httpClientFactory, _apiKeyCredential, _instanceId);
            }
            else
            {
                throw new InvalidOperationException("Either TokenCredential or ApiKeyCredential must be provided.");
            }
        }
    }
}
