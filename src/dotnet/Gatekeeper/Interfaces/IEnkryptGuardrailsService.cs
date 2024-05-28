namespace FoundationaLLM.Gatekeeper.Core.Interfaces
{
    /// <summary>
    /// Interface for calling the Enkrypt Guardrails service.
    /// </summary>
    public interface IEnkryptGuardrailsService
    {
        /// <summary>
        /// Detects attempted prompt injections and jailbreaks in user prompts.
        /// </summary>
        /// <param name="content">The text content that needs to be analyzed.</param>
        /// <returns>The text analysis restult, which includes flags that represents if the content contains prompt injections or jailbreaks.</returns>
        Task<string?> DetectPromptInjection(string content);
    }
}
