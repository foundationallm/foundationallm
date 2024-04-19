using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Models.Vectorization;
using System.Text;
using System.Web;
using System.Net;

namespace FoundationaLLM.Vectorization.Services.ContentSources
{
    /// <summary>
    /// Extracts text from a web page.
    /// </summary>
    public class WebContentSourceService : IContentSourceService
    {
        private readonly ILogger<WebContentSourceService> _logger;

        /// <summary>
        /// Creates a new instance of the vectorization content source service that scrapes web pages.
        /// </summary>
        /// <param name="loggerFactory">Logger factory that generates loggers for the class.</param>
        public WebContentSourceService(ILoggerFactory loggerFactory) =>
            _logger = loggerFactory.CreateLogger<WebContentSourceService>();

        /// <inheritdoc/>
        /// <remarks>
        /// Expected MultipartId:
        /// contentId[0] = Protocol: either http or https
        /// contentId[1] = the web URL without the protocol
        /// contentId[2] = CSS classes to filter by
        /// </remarks>
        public async Task<string> ExtractTextAsync(ContentIdentifier contentId, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            string url = $"{contentId[0]}://{contentId[1]}";

            using var httpClient = new HttpClient();
            try
            {
                var result = new StringBuilder();

                // Get the HTML content from the URL.  
                string html = await httpClient.GetStringAsync(url);

                // Load the HTML content into an HtmlDocument.  
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(html);

                var titleNode = htmlDocument.DocumentNode.SelectSingleNode("//head//title");
                if (!string.IsNullOrEmpty(titleNode.InnerText))
                    result.AppendLine(titleNode.InnerText.Trim());

                // Use XPath to select the body of the HTML document.  
                var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//body");

                if (bodyNode == null)
                {
                    throw new VectorizationException($"No body tag found in the HTML document located at: {url}.");
                }

                if (!string.IsNullOrWhiteSpace(contentId[2]))
                {
                    var cssClasses = contentId[2].Split(' ');

                    var relevantNodes = bodyNode.Descendants()
                        .Where(n =>
                            cssClasses.Any(c => n.HasClass(c))
                            && !string.IsNullOrWhiteSpace(n.InnerText))
                        .ToList();

                    var relevantContent = relevantNodes
                        .SelectMany(n => n.ChildNodes.Where(x => x.NodeType == HtmlNodeType.Element && x.Name == "p"))
                        .Select(n => WebUtility.HtmlDecode(n.InnerText))
                        .ToList();

                    result.AppendJoin(
                        Environment.NewLine,
                        relevantContent);
                }
                else
                {
                    // Remove script and style elements.  
                    htmlDocument.DocumentNode.Descendants()
                        .Where(n => n.Name == "script" || n.Name == "style")
                        .ToList()
                        .ForEach(n => n.Remove());

                    // Extract the inner text of the body element, which includes all visible text within the body.
                    // This text will not include any HTML tags.
                    string visibleText = bodyNode.InnerText;

                    // Convert multiple whitespace into a single space and trim the text.  
                    visibleText = System.Text.RegularExpressions.Regex.Replace(visibleText, @"\s+", " ").Trim();
                    result.Append(visibleText);
                }

                return result.ToString();
            }
            catch (HttpRequestException ex)
            {
                throw new VectorizationException($"Failed to retrieve content from {url}: {ex.Message}");
            }
        } 
    }
}
