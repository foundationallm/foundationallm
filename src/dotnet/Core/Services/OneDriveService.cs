using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using FoundationaLLM.Core.Interfaces;
using Microsoft.Identity.Abstractions;

namespace FoundationaLLM.Core.Services
{
    /// <summary>
    /// Implements <see cref="IOneDriveService"/>.
    /// </summary>
    public class OneDriveService : IOneDriveService
    {
        private readonly ICoreService _coreService;
        private readonly IDownstreamApi _downstreamApi;
        private readonly IUserProfileService _userProfileService;
        private readonly IHttpClientFactoryService _httpClientFactoryService;

        /// <summary>
        /// Contructor for OneDrive service.
        /// </summary>
        /// <param name="coreService">The core API service.</param>
        /// <param name="downstreamApi">The downstream API service.</param>
        /// <param name="userProfileService">The user profile service.</param>
        /// <param name="httpClientFactoryService">The HTTP client factory service.</param>
        public OneDriveService(
            ICoreService coreService,
            IDownstreamApi downstreamApi,
            IUserProfileService userProfileService,
            IHttpClientFactoryService httpClientFactoryService)
        {
            _coreService = coreService;
            _downstreamApi = downstreamApi;
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
        public async Task<ResourceProviderUpsertResult> Download(
            string instanceId, string sessionId, string agentName, string itemId, UnifiedUserIdentity userIdentity)
        {
            var userProfile = await _userProfileService.GetUserProfileAsync(instanceId);

            if (!userProfile!.Flags.TryGetValue(UserProfileFlags.OneDriveWorkSchool, out bool oneDriveWorkOrSchool)
                || !oneDriveWorkOrSchool)
                throw new InvalidOperationException("User has not granted consent to connect to the OneDrive work or school account.");

            //var client = await _httpClientFactoryService.CreateClient(HttpClientNames.OneDriveAPI, userIdentity); 

            var response = await _downstreamApi.CallApiForUserAsync("OneDrive", options =>
            {
                options.BaseUrl = $"https://graph.microsoft.com/v1.0/";
                options.RelativePath = $"me/drive/items/{itemId}/content";
                options.Scopes = [ "Files.Read.All" ];
            });

            var stream = await response.Content.ReadAsStreamAsync();

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var content = memoryStream.ToArray();

            var fileName = $"{Guid.NewGuid()}";
            var name = $"a-{fileName}-{DateTime.UtcNow.Ticks}";
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
