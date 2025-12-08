using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FoundationaLLM.Core.Services
{
    /// <summary>
    /// Implements <see cref="IOneDriveWorkSchoolService"/>.
    /// </summary>
    public class OneDriveWorkSchoolService : IOneDriveWorkSchoolService
    {
        private readonly ICoreService _coreService;
        private readonly IUserProfileService _userProfileService;
        private readonly IHttpClientFactory _httpClientFactory;

        /// <summary>
        /// Contructor for OneDrive service.
        /// </summary>
        /// <param name="coreService">The core API service.</param>
        /// <param name="userProfileService">The user profile service.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public OneDriveWorkSchoolService(
            ICoreService coreService,
            IUserProfileService userProfileService,
            IHttpClientFactory httpClientFactory)
        {
            _coreService = coreService;
            _userProfileService = userProfileService;
            _httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc/>
        public async Task Connect(string instanceId)
        {
            var userProfile = await _userProfileService.GetUserProfileAsync(instanceId);

            if (!userProfile!.Flags.TryGetValue(UserProfileFlags.OneDriveWorkSchoolEnabled, out bool oneDriveWorkOrSchoolEnabled))
                userProfile.Flags.Add(UserProfileFlags.OneDriveWorkSchoolEnabled, false);

            if (!oneDriveWorkOrSchoolEnabled)
            {
                userProfile.Flags[UserProfileFlags.OneDriveWorkSchoolEnabled] = true;

                await _userProfileService.UpsertUserProfileAsync(instanceId, userProfile);
            }
        }

        /// <inheritdoc/>
        public async Task Disconnect(string instanceId)
        {
            var userProfile = await _userProfileService.GetUserProfileAsync(instanceId);

            if (!userProfile!.Flags.TryGetValue(UserProfileFlags.OneDriveWorkSchoolEnabled, out bool oneDriveWorkOrSchoolEnabled))
                userProfile.Flags.Add(UserProfileFlags.OneDriveWorkSchoolEnabled, false);

            if (oneDriveWorkOrSchoolEnabled)
            {
                userProfile.Flags[UserProfileFlags.OneDriveWorkSchoolEnabled] = false;

                await _userProfileService.UpsertUserProfileAsync(instanceId, userProfile);
            }
        }

        /// <inheritdoc/>
        public async Task<OneDriveWorkSchoolItem> Download(
            string instanceId, string sessionId, string agentName, OneDriveWorkSchoolItem oneDriveItem, UnifiedUserIdentity userIdentity)
        {
            if (string.IsNullOrWhiteSpace(oneDriveItem.AccessToken))
                throw new InvalidOperationException("Invalid request body. Missing access token.");

            var userProfile = await _userProfileService.GetUserProfileAsync(instanceId);

            if (!userProfile!.Flags.TryGetValue(UserProfileFlags.OneDriveWorkSchoolEnabled, out bool oneDriveWorkOrSchoolEnabled)
                || !oneDriveWorkOrSchoolEnabled)
                throw new InvalidOperationException("User has not granted consent to connect to the OneDrive work or school account.");

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oneDriveItem.AccessToken);

            var item = await client.GetAsync($"drives/{oneDriveItem.DriveId}/items/{oneDriveItem.Id}");

            if (!item.IsSuccessStatusCode)
                throw new InvalidOperationException($"Could not retrieve OneDrive item information for {oneDriveItem.Id}. Status code: {item.StatusCode}.");
            var itemStr = await item.Content.ReadAsStringAsync();
            var itemObj = JsonSerializer.Deserialize<OneDriveWorkSchoolItem>(itemStr);

            var response = await client.GetAsync($"drives/{oneDriveItem.DriveId}/items/{oneDriveItem.Id}/content");

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Could not retrieve OneDrive item contents for {oneDriveItem.Id}. Status code: {response.StatusCode}.");
            var stream = await response.Content.ReadAsStreamAsync();

            var fileName = itemObj?.Name ?? Guid.NewGuid().ToString();
            var name = $"a-{Guid.NewGuid()}-{DateTime.UtcNow.Ticks}";
            var contentType = itemObj?.File?.MimeType ?? "application/octet-stream";

            var result = await _coreService.UploadAttachment(
                    instanceId,
                    sessionId,
                    new AttachmentFile
                    {
                        Name = name,
                        Content = BinaryData.FromStream(stream),
                        DisplayName = fileName,
                        ContentType = contentType,
                        OriginalFileName = fileName
                    },
                    agentName);

            return new OneDriveWorkSchoolItem()
            {
                Id = oneDriveItem.Id,
                ObjectId = result.ObjectId,
                Name = fileName,
                File = itemObj!.File,
            };
        }
    }
}
