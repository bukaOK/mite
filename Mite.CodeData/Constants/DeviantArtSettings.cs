namespace Mite.CodeData.Constants
{
    public static class DeviantArtSettings
    {
        public const string DefaultAuthType = "DeviantArt";
        public const string ClientId = "7500";
        public const string ClientSecret = "23b3b506f6461fa790dcf1225ea5b20b";
        public const string CallbackUrl = "signin-deviantart";

        public static class ApiUrls
        {
            public const string StashSubmit = "https://www.deviantart.com/api/v1/oauth2/stash/submit";
            public const string RefreshAccessToken = "https://www.deviantart.com/oauth2/token";
        }
    }
}
