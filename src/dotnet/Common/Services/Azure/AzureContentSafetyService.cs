using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.ContentSafety;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ContentSafety;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ClientModel;

namespace FoundationaLLM.Common.Services
{
    /// <summary>
    /// Implements the <see cref="IContentSafetyService"/> interface.
    /// </summary>
    public class AzureContentSafetyService : IContentSafetyService
    {
        private readonly InstanceSettings _instanceSettings;
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        private readonly AzureContentSafetySettings _settings;
        private readonly ILogger _logger;
        private readonly Task<IAzureAIContentSafetyClient> _clientTask;

        /// <summary>
        /// Constructor for the Azure Content Safety service.
        /// </summary>
        /// <param name="instanceOptions">The instance settings options.</param>
        /// <param name="httpClientFactoryService">The HTTP client factory service.</param>
        /// <param name="options">The configuration options for the Azure Content Safety service.</param>
        /// <param name="logger">The logger for the Azure Content Safety service.</param>
        public AzureContentSafetyService(
            IOptions<InstanceSettings> instanceOptions,
            IHttpClientFactoryService httpClientFactoryService,
            IOptions<AzureContentSafetySettings> options,
            ILogger<AzureContentSafetyService> logger)
        {
            _instanceSettings = instanceOptions.Value;
            _httpClientFactoryService = httpClientFactoryService;
            _settings = options.Value;
            _logger = logger;
            _clientTask = _httpClientFactoryService.CreateClient<IAzureAIContentSafetyClient>(
                _instanceSettings.Id,
                HttpClientNames.AzureContentSafety,
                ServiceContext.ServiceIdentity!,
                AzureAIContentSafetyClient.BuildClient,
                new Dictionary<string, object>()
                {
                    { HttpClientFactoryServiceKeyNames.EnableRetry, true }
                });
        }

        /// <inheritdoc/>
        public async Task<ContentSafetyAnalysisResult> AnalyzeText(string content)
        {
            var client = await _clientTask;

            AnalyzeTextResult? results = null;
            try
            {
                var clientResult = await client.AnalyzeText(new AnalyzeTextRequest
                {
                    Text = content
                });

                results = clientResult.Value;
            }
            catch (ClientResultException ex)
            {
                _logger.LogError(ex, "Azure AI Content Safety Analyze Text failed with status code: {StatusCode}, message: {Message}",
                    ex.Status, ex.Message);
                results = null;
            }

            if (results == null)
                return new ContentSafetyAnalysisResult { Success = false };

            var safe = true;
            var reason = "The prompt text did not pass the content safety filter. Reason:";

            var hateSeverity = results.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.Hate)?.Severity ?? 0;
            if (hateSeverity > _settings.HateSeverity)
            {
                reason += $" hate";
                safe = false;
            }

            var violenceSeverity = results.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.Violence)?.Severity ?? 0;
            if (violenceSeverity > _settings.ViolenceSeverity)
            {
                reason += $" violence";
                safe = false;
            }

            var selfHarmSeverity = results.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.SelfHarm)?.Severity ?? 0;
            if (selfHarmSeverity > _settings.SelfHarmSeverity)
            {
                reason += $" self-harm";
                safe = false;
            }

            var sexualSeverity = results.CategoriesAnalysis.FirstOrDefault(a => a.Category == TextCategory.Sexual)?.Severity ?? 0;
            if (sexualSeverity > _settings.SexualSeverity)
            {
                reason += $" sexual";
                safe = false;
            }

            return new ContentSafetyAnalysisResult() { Success = true, SafeContent = safe, Details = safe ? null : reason };
        }

        /// <inheritdoc/>
        public async Task<ContentSafetyAnalysisResult> DetectPromptInjection(string content)
        {
            var client = await _clientTask;

            try
            {
                var clientResult = await client.ShieldPrompt(new ShieldPromptRequest
                {
                    UserPrompt = content,
                    Documents = []
                });

                return new ContentSafetyAnalysisResult
                {
                    Success = true,
                    SafeContent = !clientResult.Value.UserPromptAnalysis.AttackDetected
                };
            }
            catch (ClientResultException ex)
            {
                _logger.LogError(ex, "Azure AI Content Safety Shield Prompt failed with status code: {StatusCode}, message: {Message}",
                    ex.Status, ex.Message);
                return new ContentSafetyAnalysisResult { Success = false };
            }
        }

        /// <inheritdoc/>
        public async Task<ContentSafetyDocumentAnalysisResult> DetectPromptInjection(
            string context,
            IEnumerable<ContentSafetyDocument> documents,
            CancellationToken cancellationToken)
        {
            var client = await _clientTask;

            try
            {
                _logger.LogInformation("Starting to process {DocumentCount} documents for prompt injection detection in context {Context}.",
                    documents.Count(), context);

                var result = new ContentSafetyDocumentAnalysisResult
                {
                    Success = true,
                    DocumentResults = documents.ToDictionary(
                        doc => doc.Id,
                        doc => new ContentSafetyAnalysisResult
                        {
                            Success = true,
                            SafeContent = true
                        })
                };

                // Group documents in work items for parallel processing
                // Each work item contains up to 5 documents with a maximum total size of 10,000 characters
                var documentGroups = CreateGroups(documents);

                await Parallel.ForEachAsync<List<ContentSafetyDocument>>(
                    documentGroups,
                    new ParallelOptions
                    {
                        CancellationToken = cancellationToken,
                        MaxDegreeOfParallelism = 10
                    },
                    async (documents, token) =>
                    {
                        bool errorOccurred = false;
                        var documentIds = string.Join(',', documents.Select(d => d.Id));

                        try
                        {
                            _logger.LogInformation(
                                "Processing documents {DocumentIds} for prompt injection in context {Context}...",
                                documentIds, context);

                            var clientResult = await client.ShieldPrompt(
                                new ShieldPromptRequest
                                {
                                    Documents = [.. documents.Select(d => d.Content)]
                                },
                                cancellationToken);

                            for (int i = 0; i < documents.Count; i++)
                            {
                                var document = documents[i];
                                var analysis = clientResult.Value.DocumentsAnalysis[i];
                                result.DocumentResults[document.Id].Success = true;
                                result.DocumentResults[document.Id].SafeContent = !analysis.AttackDetected;
                            }
                        }
                        catch (ClientResultException ex)
                        {
                            _logger.LogError(
                                ex,
                                "There was a service client error processing documents {DocumentIds} for prompt injection in context {Context}",
                                documentIds, context);

                            errorOccurred = true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(
                                ex,
                                "There was an error processing documents {DocumentIds} for prompt injection in context {Context}",
                                documentIds, context);

                            errorOccurred = true;
                        }

                        if (errorOccurred)
                            foreach (var document in documents)
                            {
                                result.DocumentResults[document.Id].Success = false;
                                result.DocumentResults[document.Id].SafeContent = false;
                            }
                    });

                _logger.LogInformation("Finished processing documents for prompt injection detection in context {Context}.",
                    context);

                return result;
            }
            catch (ContentSafetyException ex)
            {
                _logger.LogError(ex, "The Azure AI Content Safety service reported an error while processing documents for prompt injection in context {Context}.",
                    context);
                return new ContentSafetyDocumentAnalysisResult
                {
                    Success = false,
                    Details = $"The Azure AI Content Safety service reported an error while processing documents for prompt injection in context {context}: {ex.Message}",
                    DocumentResults = []
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The Azure AI Content Safety service encountered an error while processing documents for prompt injection in context {Context}.",
                    context);
                return new ContentSafetyDocumentAnalysisResult
                {
                    Success = false,
                    Details = $"The Azure AI Content Safety service encountered an error while processing documents for prompt injection in context {context}.",
                    DocumentResults = []
                };
            }
        }

        static List<List<ContentSafetyDocument>> CreateGroups(IEnumerable<ContentSafetyDocument> docs)
        {
            const int MaxPerGroup = 5;
            const int MaxCharsPerGroup = 10_000;

            var groups = new List<List<ContentSafetyDocument>>();
            var current = new List<ContentSafetyDocument>();
            var currentSize = 0;

            foreach (var doc in docs)
            {
                var docSize = doc.Content?.Length ?? 0;

                // A document exceeding the max allowed size is not allowed.
                if (docSize > MaxCharsPerGroup)
                    throw new ContentSafetyException(
                        $"Document with id {doc.Id} exceeds the maximum allowed size of {MaxCharsPerGroup} characters.");

                var wouldExceedSize = currentSize + docSize > MaxCharsPerGroup;
                var wouldExceedCount = current.Count + 1 > MaxPerGroup;

                if (wouldExceedSize || wouldExceedCount)
                {
                    // close current group and start a new one
                    if (current.Count > 0)
                    {
                        groups.Add(current);
                        current = [];
                        currentSize = 0;
                    }
                }

                current.Add(doc);
                currentSize += docSize;
            }

            if (current.Count > 0)
                groups.Add(current);

            return groups;
        }
    }
}
