using Integration.Yandex.Maps.Models;

namespace Integration.Yandex.Maps.Services
{
    internal class MapsApi
    {
        private YandexMapConfiguration _configuration;
        private HttpClient _client;
        public async Task PointSearch(string address) 
        {
            var url = new Uri($"{_configuration.BaseUrl}/1.x?apikey={_configuration.ApiKey}&geocode={address}&results={1}");
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new Exception();


        }
    }
}
