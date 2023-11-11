using Integration.Google.Maps.Models.Response;

namespace Integration.Google.Maps.Interfaces
{
    public interface IGoogleMaps
    {
        Task<string> GeocodeAddress(string address,CancellationToken ctn = default);
        Task<MapElement[]> CalculateDistances(IReadOnlyCollection<string> startAddresses, IReadOnlyCollection<string> destinations, CancellationToken ctn = default);
    }
}
