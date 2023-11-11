namespace Integration.Google.Maps
{
    internal class GoogleMapsConfiguration
    {
        public readonly static string ConfigurationSection = nameof(GoogleMapsConfiguration);

        public required string ApiUrl { get; set; }
        public required string ApiKey { get; set; }
    }
}
