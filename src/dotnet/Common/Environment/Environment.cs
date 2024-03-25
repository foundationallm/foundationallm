namespace FoundationaLLM
{
    /// <summary>
    /// Provides methods for working with the environment.
    /// </summary>
    public static class ValidatedEnvironment
    {
        /// <summary>
        /// Gets the name of the machine and falls back to a random GUID if the machine name is not available.
        /// </summary>
        public static string MachineName =>
            string.IsNullOrWhiteSpace(Environment.MachineName) ? Guid.NewGuid().ToString() : Environment.MachineName;
    }
}
