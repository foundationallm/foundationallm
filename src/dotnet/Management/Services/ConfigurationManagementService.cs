﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Data.AppConfiguration;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Management.Interfaces;
using FoundationaLLM.Management.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Management.Services
{
    /// <inheritdoc/>
    public class ConfigurationManagementService : IConfigurationManagementService
    {
        private readonly ILogger<ConfigurationManagementService> _logger;
        private readonly AppConfigurationSettings _appConfigurationSettings;
        private readonly ConfigurationClient _configurationClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationManagementService"/> class.
        /// </summary>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="ConfigurationManagementService"/> type name.</param>
        /// <param name="appConfigurationSettings">The application configuration settings.</param>
        public ConfigurationManagementService(ILogger<ConfigurationManagementService> logger,
            IOptions<AppConfigurationSettings> appConfigurationSettings)
        {
            _logger = logger;
            _appConfigurationSettings = appConfigurationSettings.Value;
            _configurationClient = new ConfigurationClient(_appConfigurationSettings.ConnectionString);
        }

        /// <inheritdoc/>
        public async Task<List<string>?> GetAgentHintsAsync()
        {
            var agentHints = new List<string>();
            try
            {
                var agentHintsSetting = await _configurationClient
                    .GetConfigurationSettingAsync(AppConfigurationKeys.FoundationaLLM_Branding_AllowAgentSelection);
                if (agentHintsSetting.HasValue)
                {
                    agentHints = agentHintsSetting.Value.Value.Split(',').ToList();
                    agentHints = agentHints.Select(agentHint => agentHint.Trim()).ToList();
                }
                else
                {
                    agentHints = null;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting agent hints from app configuration.");
            }

            return agentHints;
        }

        /// <inheritdoc/>
        public async Task UpdateAgentHintsAsync(IEnumerable<string>? agentHints)
        {
            try
            {
                var agentHintsSettingValue = "";
                if (agentHints != null)
                {
                    agentHintsSettingValue = string.Join(", ", agentHints);
                }
                var agentHintsSetting = new ConfigurationSetting(AppConfigurationKeys.FoundationaLLM_Branding_AllowAgentSelection, agentHintsSettingValue);
                await _configurationClient.SetConfigurationSettingAsync(agentHintsSetting);
                // TODO: Restart the Core API and Agent API services to apply the agent hint value updates.
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error updating agent hints in app configuration.");
            }
        }

        /// <inheritdoc/>
        public async Task<bool> GetAllowAgentSelectionAsync()
        {
            var allowAgentSelection = false;
            try
            {
                var allowAgentSelectionSetting = await _configurationClient
                    .GetConfigurationSettingAsync(FeatureFlagConfigurationSetting.KeyPrefix + AppConfigurationKeys.FoundationaLLM_AllowAgentHint_FeatureFlag);
                if (allowAgentSelectionSetting.HasValue && allowAgentSelectionSetting.Value is FeatureFlagConfigurationSetting featureFlag)
                {
                    allowAgentSelection = featureFlag.IsEnabled;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting allow agent selection feature flag from app configuration.");
            }

            return allowAgentSelection;
        }

        /// <inheritdoc/>
        public async Task SetAllowAgentSelectionAsync(bool allowAgentSelection)
        {
            try
            {
                var allowAgentSelectionSetting = new FeatureFlagConfigurationSetting(
                    AppConfigurationKeys.FoundationaLLM_AllowAgentHint_FeatureFlag, isEnabled: allowAgentSelection);
                await _configurationClient.SetConfigurationSettingAsync(allowAgentSelectionSetting);
                // TODO: Restart the Core API and Agent API services to apply the new feature flag.
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error setting allow agent selection feature flag in app configuration.");
            }
        }

        /// <inheritdoc/>
        public async Task<ClientBrandingConfiguration> GetBrandingConfigurationAsync()
        {
            var brandingConfiguration = new ClientBrandingConfiguration();
            try
            {
                var selector = new SettingSelector { KeyFilter = AppConfigurationKeyFilters.FoundationaLLM_Branding };
                await foreach (var setting in _configurationClient.GetConfigurationSettingsAsync(selector))
                {
                    switch (setting.Key)
                    {
                        case AppConfigurationKeys.FoundationaLLM_Branding_AccentColor:
                            brandingConfiguration.AccentColor = setting.Value;
                            break;
                        case AppConfigurationKeys.FoundationaLLM_Branding_BackgroundColor:
                            brandingConfiguration.BackgroundColor = setting.Value;
                            break;
                        case AppConfigurationKeys.FoundationaLLM_Branding_CompanyName:
                            brandingConfiguration.CompanyName = setting.Value;
                            break;
                        case AppConfigurationKeys.FoundationaLLM_Branding_FavIconUrl:
                            brandingConfiguration.FavIconUrl = setting.Value;
                            break;
                        case AppConfigurationKeys.FoundationaLLM_Branding_KioskMode:
                            brandingConfiguration.KioskMode = bool.Parse(setting.Value);
                            break;
                        case AppConfigurationKeys.FoundationaLLM_Branding_LogoText:
                            brandingConfiguration.LogoText = setting.Value;
                            break;
                        case AppConfigurationKeys.FoundationaLLM_Branding_LogoUrl:
                            brandingConfiguration.LogoUrl = setting.Value;
                            break;
                        case AppConfigurationKeys.FoundationaLLM_Branding_PageTitle:
                            brandingConfiguration.PageTitle = setting.Value;
                            break;
                        case AppConfigurationKeys.FoundationaLLM_Branding_PrimaryColor:
                            brandingConfiguration.PrimaryColor = setting.Value;
                            break;
                        case AppConfigurationKeys.FoundationaLLM_Branding_PrimaryTextColor:
                            brandingConfiguration.PrimaryTextColor = setting.Value;
                            break;
                        case AppConfigurationKeys.FoundationaLLM_Branding_SecondaryColor:
                            brandingConfiguration.SecondaryColor = setting.Value;
                            break;
                        case AppConfigurationKeys.FoundationaLLM_Branding_SecondaryTextColor:
                            brandingConfiguration.SecondaryTextColor = setting.Value;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting branding configuration from app configuration.");
            }

            return brandingConfiguration;
        }

        /// <inheritdoc/>
        public async Task SetBrandingConfiguration(ClientBrandingConfiguration brandingConfiguration)
        {
            try
            {
                var settings = new List<ConfigurationSetting>
                {
                    new(AppConfigurationKeys.FoundationaLLM_Branding_AccentColor,
                        brandingConfiguration.AccentColor),
                    new(AppConfigurationKeys.FoundationaLLM_Branding_BackgroundColor,
                        brandingConfiguration.BackgroundColor),
                    new(AppConfigurationKeys.FoundationaLLM_Branding_CompanyName,
                        brandingConfiguration.CompanyName),
                    new(AppConfigurationKeys.FoundationaLLM_Branding_FavIconUrl,
                        brandingConfiguration.FavIconUrl),
                    new(AppConfigurationKeys.FoundationaLLM_Branding_KioskMode,
                        brandingConfiguration.KioskMode.ToString()),
                    new(AppConfigurationKeys.FoundationaLLM_Branding_LogoText,
                        brandingConfiguration.LogoText),
                    new(AppConfigurationKeys.FoundationaLLM_Branding_LogoUrl,
                        brandingConfiguration.LogoUrl),
                    new(AppConfigurationKeys.FoundationaLLM_Branding_PageTitle,
                        brandingConfiguration.PageTitle),
                    new(AppConfigurationKeys.FoundationaLLM_Branding_PrimaryColor,
                        brandingConfiguration.PrimaryColor),
                    new(AppConfigurationKeys.FoundationaLLM_Branding_PrimaryTextColor,
                        brandingConfiguration.PrimaryTextColor),
                    new(AppConfigurationKeys.FoundationaLLM_Branding_SecondaryColor,
                        brandingConfiguration.SecondaryColor),
                    new(AppConfigurationKeys.FoundationaLLM_Branding_SecondaryTextColor,
                        brandingConfiguration.SecondaryTextColor)
                };
                foreach (var setting in settings)
                {
                    await _configurationClient.SetConfigurationSettingAsync(setting);
                }
                // TODO: Restart the Core API to apply the new branding configuration.
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error setting branding configuration in app configuration.");
            }
        }
    }
}
