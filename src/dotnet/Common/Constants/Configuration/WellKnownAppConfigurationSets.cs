namespace FoundationaLLM.Common.Constants.Configuration
{
    /// <summary>
    /// Defines well-known application configuration sets.
    /// </summary>
    public static class WellKnownAppConfigurationSets
    {
        /// <summary>
        /// Gets the well-known user portal configuration sets, grouped by type.
        /// </summary>
        public static readonly (List<string> StringTypes, List<string> IntTypes, List<string> BoolTypes) UserPortal =
            (
                // String values
                [
                    AppConfigurationKeys.FoundationaLLM_Instance_Id,
                    AppConfigurationKeys.FoundationaLLM_Instance_StatusMessage,
                    AppConfigurationKeys.FoundationaLLM_APIEndpoints_CoreAPI_Essentials_APIUrl,
                    AppConfigurationKeys.FoundationaLLM_APIEndpoints_ContextAPI_Configuration_FileService_AllowedFileExtensions,
                    // Branding
                    AppConfigurationKeys.FoundationaLLM_Branding_PageTitle,
                    AppConfigurationKeys.FoundationaLLM_Branding_FavIconUrl,
                    AppConfigurationKeys.FoundationaLLM_Branding_LogoUrl,
                    AppConfigurationKeys.FoundationaLLM_Branding_LogoText,
                    AppConfigurationKeys.FoundationaLLM_Branding_BackgroundColor,
                    AppConfigurationKeys.FoundationaLLM_Branding_PrimaryColor,
                    AppConfigurationKeys.FoundationaLLM_Branding_SecondaryColor,
                    AppConfigurationKeys.FoundationaLLM_Branding_AccentColor,
                    AppConfigurationKeys.FoundationaLLM_Branding_PrimaryTextColor,
                    AppConfigurationKeys.FoundationaLLM_Branding_SecondaryTextColor,
                    AppConfigurationKeys.FoundationaLLM_Branding_AccentTextColor,
                    AppConfigurationKeys.FoundationaLLM_Branding_PrimaryButtonBackgroundColor,
                    AppConfigurationKeys.FoundationaLLM_Branding_PrimaryButtonTextColor,
                    AppConfigurationKeys.FoundationaLLM_Branding_SecondaryButtonBackgroundColor,
                    AppConfigurationKeys.FoundationaLLM_Branding_SecondaryButtonTextColor,
                    AppConfigurationKeys.FoundationaLLM_Branding_FooterText,
                    AppConfigurationKeys.FoundationaLLM_Branding_NoAgentsMessage,
                    AppConfigurationKeys.FoundationaLLM_Branding_DefaultAgentWelcomeMessage,
                    AppConfigurationKeys.FoundationaLLM_Branding_AgentIconUrl,
                    // User portal configuration
                    AppConfigurationKeys.FoundationaLLM_UserPortal_Configuration_AgentManagementPermissionRequestUrl,
                    AppConfigurationKeys.FoundationaLLM_UserPortal_Configuration_FeaturedAgentNames
                ],
                // Integer values
                [
                ],
                // Boolean values
                [
                    // Branding
                    AppConfigurationKeys.FoundationaLLM_Branding_KioskMode,
                    // User portal configuration
                    AppConfigurationKeys.FoundationaLLM_UserPortal_Configuration_ShowMessageRating,
                    AppConfigurationKeys.FoundationaLLM_UserPortal_Configuration_ShowLastConversationOnStartup,
                    AppConfigurationKeys.FoundationaLLM_UserPortal_Configuration_ShowMessageTokens,
                    AppConfigurationKeys.FoundationaLLM_UserPortal_Configuration_ShowViewPrompt,
                    AppConfigurationKeys.FoundationaLLM_UserPortal_Configuration_ShowFileUpload
                ]
            );

        /// <summary>
        /// Gets the well-known management portal configuration sets, grouped by type.
        /// </summary>
        public static readonly (List<string> StringTypes, List<string> IntTypes, List<string> BoolTypes) ManagementPortal =
            (
                // String values
                [
                ],
                // Integer values
                [
                ],
                // Boolean values
                [
                ]
            );

        /// <summary>
        /// Gets all well-known application configuration sets, grouped by name.
        /// </summary>
        public static readonly Dictionary<string, (List<string> StringTypes, List<string> IntTypes, List<string> BoolTypes)> All =
            new()
            {
                { WellKnownAppConfigurationSetNames.UserPortal, UserPortal },
                { WellKnownAppConfigurationSetNames.ManagementPortal, ManagementPortal }
            };
    }
}
