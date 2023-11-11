using Integration.Google.Maps.Interfaces;
using Integration.Google.Maps.Models.Response;
using System.Text.Json;

namespace Integration.Google.Maps.Services
{
    internal class GoogleMaps : IGoogleMaps
    {
        private HttpClient _client;
        private GoogleMapsConfiguration _settings;

        public GoogleMaps(HttpClient httpClient, GoogleMapsConfiguration settings)
        {
            _client = httpClient;
            _settings = settings;
        }

        public async Task<string> GeocodeAddress(string address, CancellationToken ctn = default)
        {
            var url = $"{_settings.ApiUrl}json?address={address}&key={_settings.ApiKey}&region=RU";

            var response = await _client.GetAsync(url, ctn);
            var responseBody = await response.Content.ReadAsStringAsync(ctn);
            var jsonResponse = JsonSerializer.Deserialize<GoogleMapsGeocoderResponse>(responseBody);

            var result = jsonResponse!.results.FirstOrDefault();

            return $"{result!.geometry.location.lat},{result.geometry.location.lng}";
        }

        public async Task<MapElement[]> CalculateDistances(IReadOnlyCollection<string> origins, IReadOnlyCollection<string> destinations, CancellationToken ctn = default)
        {
            var result = new List<MapElement>();
            foreach (var origin in origins)
            {
                var roads = new List<MapElement.Road>();
                foreach (var destination in destinations.Chunk(25))
                {
                    var url = $"{_settings.ApiUrl}distancematrix/json?origins={string.Join("|", origin)}&destinations={string.Join("|", destination)}&key={_settings.ApiKey}&region=RU";
                    var response = await _client.GetAsync(url, ctn);
                    var responseBody = await response.Content.ReadAsStringAsync(ctn);
                    var jsonResponse = JsonSerializer.Deserialize<GoogleMapsDistancesResponse>(responseBody);

                    var row = jsonResponse!.rows!.FirstOrDefault();

                    var rows = row!.elements!.Select((road, i) => new MapElement.Road
                    {
                        Address = destinations.ElementAt(i),
                        Distance = road.distance!.value,
                        Duration = road.duration!.value
                    });
                    roads.AddRange(rows);
                }
                result.Add(new MapElement
                {
                    Address = origin,
                    Roads = roads
                });
            }
            return result.ToArray();
        }
    }
}
