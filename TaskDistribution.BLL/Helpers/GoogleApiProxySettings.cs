namespace TaskDistribution.BLL.Helpers
{
    internal class GoogleApiProxySettings
    {
        public readonly static string ConfigurationSection = nameof(GoogleApiProxySettings);

        public required bool UseCache { get; init; }
    }
}
