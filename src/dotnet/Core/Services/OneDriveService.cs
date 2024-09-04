using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Core.Interfaces;

namespace FoundationaLLM.Core.Services
{
    /// <summary>
    /// Implements <see cref="IOneDriveService"/>.
    /// </summary>
    public class OneDriveService : IOneDriveService
    {
        private readonly IUserProfileService _userProfileService;
        private readonly IHttpClientFactoryService _httpClientFactoryService;

        /// <summary>
        /// Contructor for OneDrive service.
        /// </summary>
        /// <param name="userProfileService">Service that provides methods for managing the user profile.</param>
        /// <param name="httpClientFactoryService">The HTTP client factory service.</param>
        public OneDriveService(
            IUserProfileService userProfileService,
            IHttpClientFactoryService httpClientFactoryService)
        {
            _userProfileService = userProfileService;
            _httpClientFactoryService = httpClientFactoryService;
        }

        /// <inheritdoc/>
        public async Task Connect(string instanceId)
        {
            var userProfile = await _userProfileService.GetUserProfileAsync(instanceId);

            if (!userProfile!.Flags.TryGetValue(UserProfileFlags.OneDriveWorkSchool, out bool oneDriveWorkOrSchool))
                userProfile.Flags.Add(UserProfileFlags.OneDriveWorkSchool, false);

            if (!oneDriveWorkOrSchool)
            {
                userProfile.Flags[UserProfileFlags.OneDriveWorkSchool] = true;

                await _userProfileService.UpsertUserProfileAsync(instanceId, userProfile);
            }
        }

        /// <inheritdoc/>
        public async Task Disconnect(string instanceId)
        {
            var userProfile = await _userProfileService.GetUserProfileAsync(instanceId);

            if (!userProfile!.Flags.TryGetValue(UserProfileFlags.OneDriveWorkSchool, out bool oneDriveWorkOrSchool))
                userProfile.Flags.Add(UserProfileFlags.OneDriveWorkSchool, false);

            if (oneDriveWorkOrSchool)
            {
                userProfile.Flags[UserProfileFlags.OneDriveWorkSchool] = false;

                await _userProfileService.UpsertUserProfileAsync(instanceId, userProfile);
            }
        }

        /// <inheritdoc/>
        public async Task<string> Download(string instanceId, string itemId, UnifiedUserIdentity userIdentity)
        {
            var userProfile = await _userProfileService.GetUserProfileAsync(instanceId);

            if (!userProfile!.Flags.TryGetValue(UserProfileFlags.OneDriveWorkSchool, out bool oneDriveWorkOrSchool)
                || !oneDriveWorkOrSchool)
                throw new InvalidOperationException("User has not granted consent to connect to the OneDrive work or school account.");

            var client = await _httpClientFactoryService.CreateClient(HttpClientNames.OneDriveAPI, userIdentity);

            var response = await client.GetAsync($"/{itemId}/content");

            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
    }
}
