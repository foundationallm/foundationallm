namespace FoundationaLLM.Common.Models.Configuration.Environment
{
    /// <summary>
    /// Represents a single entry in the environment variable catalog.
    /// </summary>
    public class EnvironmentVariableEntry(string name, string description, string? defaultValue = null)
    {
        /// <summary>
        /// The name of the environment variable.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// A description of the environment variable.
        /// </summary>
        public string Description { get; } = description;

        /// <summary>
        /// The default value of the environment variable, if any.
        /// </summary>
        public string? DefaultValue { get; } = defaultValue;
    }
}
