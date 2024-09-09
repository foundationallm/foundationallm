using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FoundationaLLM.Core.Services
{
    /// <summary>
    /// Implements <see cref="IOneDriveService"/>.
    /// </summary>
    public class OneDriveService : IOneDriveService
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
        public OneDriveService(
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
        public async Task<ResourceProviderUpsertResult> Download(
            string instanceId, string sessionId, string agentName, OneDriveItem oneDriveItem, UnifiedUserIdentity userIdentity)
        {
            var userProfile = await _userProfileService.GetUserProfileAsync(instanceId);

            if (!userProfile!.Flags.TryGetValue(UserProfileFlags.OneDriveWorkSchool, out bool oneDriveWorkOrSchool)
                || !oneDriveWorkOrSchool)
                throw new InvalidOperationException("User has not granted consent to connect to the OneDrive work or school account.");

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", oneDriveItem.AccessToken);
;
            var item = await client.GetAsync($"me/drive/items/{oneDriveItem.Id}");
            var itemStr = await item.Content.ReadAsStringAsync();
            var itemObj = JsonSerializer.Deserialize<OneDriveItem>(itemStr);

            var response = await client.GetAsync($"me/drive/items/{oneDriveItem.Id}/content");
            var stream = await response.Content.ReadAsStreamAsync();

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var content = memoryStream.ToArray();

            var fileName = itemObj!.Name;
            var name = $"a-{Guid.NewGuid()}-{DateTime.UtcNow.Ticks}";
            var contentType = "application/octet-stream";

            var result = await _coreService.UploadAttachment(
                    instanceId,
                    sessionId,
                    new AttachmentFile
                    {
                        Name = name,
                        Content = content,
                        DisplayName = fileName,
                        ContentType = contentType,
                        OriginalFileName = fileName
                    },
                    agentName,
                    userIdentity);

            return result;
        }
    }
}
