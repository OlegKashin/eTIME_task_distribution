namespace Integration.Yandex.Maps.Models
{
    internal record YandexMapConfiguration
    {
        public required string ApiKey { get; set; }
        public required string BaseUrl { get; set; }
    }
}
