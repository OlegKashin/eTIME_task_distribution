using EtaiEcoSystem.Notification.Shell;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskDistribution.BLL.Interfaces;

namespace TaskDistribution.BLL
{
    public static class Configure 
    {
        public static IServiceCollection AddTaskDistributionBLL(this IServiceCollection services)
        {

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            services.AddScoped<IBusinessManager, BusinessManager>();
            services.AddScoped<IGoogleApiProxy, GoogleApiProxy>();
            services.Configure<GoogleApiProxySettings>(configuration.GetSection(GoogleApiProxySettings.ConfigurationSection));

            return services;
        }
    }

}
