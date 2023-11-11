using EtaiEcoSystem.EventBus.Models.DTOs.Base;

namespace TaskDistribution.BLL.Interfaces
{
    public interface IDistributionService
    {
        Task DistributionTask(BaseBoardRequest request, CancellationToken ctn = default);
    }
}
