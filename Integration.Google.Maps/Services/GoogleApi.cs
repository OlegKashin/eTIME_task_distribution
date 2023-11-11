using Integration.Google.Maps.Interfaces;
using Microsoft.Extensions.Options;

namespace Integration.Google.Maps.Services
{
    internal class GoogleApi : IGoogleApi
    {
        private GoogleMapsConfiguration _settings;
        private HttpClient _client;

        public IGoogleMaps GoogleMaps { get; }

        public GoogleApi(IOptions<GoogleMapsConfiguration> settings, HttpClient client)
        {
            _settings = settings.Value;
            _client = client;

            GoogleMaps = new GoogleMaps(_client, _settings);
        }
    }
}
