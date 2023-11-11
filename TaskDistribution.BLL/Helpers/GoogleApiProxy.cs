using EtaiEcoSystem.RedisCache.Interfaces;
using EtaiEcoSystem.RedisCache.Models;
using Integration.Google.Maps.Interfaces;
using Integration.Google.Maps.Models.Response;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using TaskDistribution.BLL.Interfaces;

namespace TaskDistribution.BLL.Helpers
{
    internal class GoogleApiProxy : IGoogleMaps, IGoogleApiProxy
    {
        private readonly GoogleApiProxySettings settings;
        private readonly IGoogleApi googleApi;
        private readonly IRedisCache redisCache;

        public GoogleApiProxy(IGoogleApi googleApi, IRedisCache redisCache, IOptions<GoogleApiProxySettings> settings)
        {
            this.googleApi = googleApi;
            this.redisCache = redisCache;
            this.settings = settings.Value;
        }

        public async Task<string> GeocodeAddress(string address, CancellationToken ctn = default)
        {
            if (!settings.UseCache) 
                return await googleApi.GoogleMaps.GeocodeAddress(address, ctn);

            var cacheInstance = redisCache.GetCache<GeocodeCacheInstance2>(address);
            if (await cacheInstance.IsExistAsync())
            {
                var data = await cacheInstance.GetAsync();

                return data.Geocode;
            }

            var result = await googleApi.GoogleMaps.GeocodeAddress(address, ctn);
            await cacheInstance.SetAsync(new GeocodeCacheModel { Geocode = result });
            return result;
        }

        public async Task<MapElement[]> CalculateDistances(IReadOnlyCollection<string> startAddresses, IReadOnlyCollection<string> destinations, CancellationToken ctn = default)
        {
            if (!settings.UseCache)
                return await googleApi.GoogleMaps.CalculateDistances(startAddresses, destinations, ctn);

            var cacheInstance = redisCache.GetCache<DistancesCacheInstance2>();
            if (await cacheInstance.IsExistAsync())
            {
                var data = await cacheInstance.GetAsync();

                return data.Data;
            }

            var result = await googleApi.GoogleMaps.CalculateDistances(startAddresses, destinations, ctn);
            await cacheInstance.SetAsync(new DistancesCacheModel { Data = result });
            return result;
        }

        public class GeocodeCacheInstance2 : BaseCacheModel<GeocodeCacheModel>
        {
            public GeocodeCacheInstance2(string cacheKey, IDistributedCache cache) : base(cacheKey, cache)
            {
                DefaultOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
            }
        }

        public record GeocodeCacheModel
        {
            public const string CacheModelCode = nameof(GeocodeCacheModel);

            public required string Geocode { get; init; }
        }

        public class DistancesCacheInstance2 : BaseCacheModel<DistancesCacheModel>
        {
            public DistancesCacheInstance2(string cacheKey, IDistributedCache cache) : base(cacheKey, cache)
            {
                DefaultOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);
            }
        }

        public record DistancesCacheModel
        {
            public const string CacheModelCode = nameof(DistancesCacheModel);

            public required MapElement[] Data { get; init; }
        }
    }
}
