using Azure.Core;
using Azure.Core.Pipeline;
using System.Text;

namespace FoundationaLLM.Common.Clients.Http
{
    /// <summary>
    /// Represents a policy that intercepts HTTP pipeline requests and responses for custom processing.
    /// </summary>
    /// <remarks>This policy can be used to modify or inspect HTTP requests and responses as they pass through
    /// the pipeline. It is typically used for logging, monitoring, or altering request and response data.</remarks>
    public class HttpPipelineInterceptPolicy : HttpPipelineSynchronousPolicy
    {
        /// <inheritdoc/>
        public override void OnSendingRequest(HttpMessage message)
        {
            if (message.Request.Content != null)
            {
                using var memoryStream = new MemoryStream();
                message.Request.Content.WriteTo(memoryStream, default);
                memoryStream.Position = 0;
                string contentString = Encoding.UTF8.GetString(memoryStream.ToArray());

                // You can now use contentString for logging or inspection
            }
        }

        /// <inheritdoc/>
        public override void OnReceivedResponse(HttpMessage message)
        {
            if (message.Response.Content != null)
            {
                var contentString = message.Response.Content.ToString();

                // You can now use contentString for logging or inspection
            }
        }
    }
}
