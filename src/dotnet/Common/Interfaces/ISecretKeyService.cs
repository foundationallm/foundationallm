namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides services to generate and validate secret keys.
    /// </summary>
    public interface ISecretKeyService
    {
        string GenerateKey();
    }
}
