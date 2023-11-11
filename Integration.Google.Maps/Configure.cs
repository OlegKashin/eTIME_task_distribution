using Integration.Google.Maps.Interfaces;
using Integration.Google.Maps.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Google.Maps
{
    public static class Configure
    {
        public static IServiceCollection AddGoogleApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GoogleMapsConfiguration>(configuration.GetSection(GoogleMapsConfiguration.ConfigurationSection));

            services.AddSingleton<IGoogleApi, GoogleApi>();

            return services;
        }
    }
}
