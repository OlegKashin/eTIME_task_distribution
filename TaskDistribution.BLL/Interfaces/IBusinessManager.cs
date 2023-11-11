using TaskDistribution.BLL.Services;

namespace TaskDistribution.BLL.Interfaces
{
    public interface IBusinessManager
    {
        public IDistributionService Distribution { get; }
        public IImportService ImportService { get; }
    }
}
