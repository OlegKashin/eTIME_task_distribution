using EtaiEcoSystem.EventBus.Interfaces;
using EtaiEcoSystem.RedisCache.Interfaces;
using Integration.Google.Maps.Interfaces;
using TaskDistribution.BLL.Interfaces;
using TaskDistribution.BLL.Services;

namespace TaskDistribution.BLL
{
    internal class BusinessManager : IBusinessManager
    {
        internal required IGoogleApi GoogleApi { get; init; }
        internal required IBusManager BusManager { get; init; }
        internal required IRedisCache Cache { get; init; }

        private IDistributionService? _distributionService;
        private IImportService? _importService;

        public IDistributionService Distribution => _distributionService ??= new DistributionService(this);
        public IImportService ImportService => _importService ??= new ImportService(this);
    }
}
