namespace FoundationaLLM.Common.Constants
{
    public static class UserProfileFlags
    {
        /// <summary>
        /// Flag that allows the user to grant or revoke consent to connect to their OneDrive for work or school account.
        /// </summary>
        public const string OneDriveWorkSchool = "OneDriveWorkSchool";

        /// <summary>
        /// All user profile flags.
        /// </summary>
        public readonly static string[] All = [
            OneDriveWorkSchool
        ];
    }
}
