using EtaiEcoSystem.EventBus.Enums.Models.TaskManager;
using EtaiEcoSystem.EventBus.Models.DTOs.TaskDistribution;
using EtaiEcoSystem.EventBus.Models.DTOs.TaskManager.Requests.CardRequests;
using EtaiEcoSystem.EventBus.Models.DTOs.TaskManager.Requests.CardStatusTypeRequests;
using EtaiEcoSystem.EventBus.Models.DTOs.TaskManager.Requests.CardTypeRequests;
using EtaiEcoSystem.EventBus.Models.DTOs.TaskManager.Responses.CardResponses;
using EtaiEcoSystem.EventBus.Models.DTOs.TaskManager.Responses.CardStatusTypeReponses;
using EtaiEcoSystem.EventBus.Models.DTOs.TaskManager.Responses.CardTypeReponses;
using Org.BouncyCastle.Ocsp;
using TaskDistribution.BLL.Helpers;
using TaskDistribution.BLL.Interfaces;
using TaskDistribution.BLL.Models;
using Task = System.Threading.Tasks.Task;

namespace TaskDistribution.BLL.Services
{
    internal class ImportService : IImportService
    {
        private readonly BusinessManager _bll;

        public ImportService(BusinessManager bll)
        {
            _bll = bll;
        }

        public async Task Import(ImportRequest request, CancellationToken ctn = default)
        {
            IReadOnlyCollection<ImportData> importData = null!;
            using (var stream = request.File.OpenReadStream())
                importData = ExcelFileImporter.Parse(stream);

            var cardTypes = await _bll.BusManager.SendAsync<GetCardTypesRequest, GetCardTypesResponse>(new GetCardTypesRequest
            {
                AuthorizedUserId = request.AuthorizedUserId,
                WorkspaceId = request.WorkspaceId,
                ProjectId = request.ProjectId,
                BoardId = request.BoardId
            });

            var cardStatusTypes = await _bll.BusManager.SendAsync<GetCardStatusTypesRequest, GetCardStatusTypesResponse>(new GetCardStatusTypesRequest
            {
                AuthorizedUserId = request.AuthorizedUserId,
                WorkspaceId = request.WorkspaceId,
                ProjectId = request.ProjectId,
                BoardId = request.BoardId,
            });

            var defaultCardType = cardTypes.Message.CardTypes.FirstOrDefault();
            var defaultCardStatusType = cardStatusTypes.Message.CardStatusTypes.FirstOrDefault();

            foreach (var data in importData)
            {
                await _bll.BusManager.SendAsync<CreateCardRequest, CreateCardResponse>(new CreateCardRequest
                {
                    AuthorizedUserId = request.AuthorizedUserId,
                    WorkspaceId = request.WorkspaceId,
                    ProjectId = request.ProjectId,
                    BoardId = request.BoardId,
                    CardId = null,
                    Data = new CreateCardRequestData
                    {
                        SprintId = null,
                        CardTypeId = defaultCardType!.CardTypeId,
                        CardStatusTypeId = defaultCardStatusType!.CardStatusTypeId,
                        Priority = (CardPriority)data.Priority,
                        TimeEstimate = data.Time,
                        Title = data.Address,
                        Description = data.Description,
                    }
                });
            }
        }
    }
}
