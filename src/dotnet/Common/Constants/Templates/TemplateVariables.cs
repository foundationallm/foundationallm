namespace FoundationaLLM.Common.Constants.Templates
{
    /// <summary>
    /// Provides template variables that can be used in templates.
    /// </summary>
    public static class TemplateVariables
    {
        /// <summary>
        /// Token for current date in UTC format.
        /// </summary>
        public const string CurrentDateTimeUTC = "current_datetime_utc";

        /// <summary>
        /// Token for a workflow router prompt.
        /// </summary>
        public const string RouterPrompt = "router_prompt";

        /// <summary>
        /// Token for the router prompt tools list.
        /// </summary>
        public const string PromptToolsList = "prompt_tools_list";
    }
}
