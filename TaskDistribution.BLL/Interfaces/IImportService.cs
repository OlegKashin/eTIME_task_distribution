using Common.Requests;

namespace TaskDistribution.BLL.Interfaces
{
    public interface IImportService
    {
        Task Import(ImportRequest importRequest, CancellationToken ctn = default);
    }
}
