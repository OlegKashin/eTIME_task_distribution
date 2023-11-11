using EtaiEcoSystem.Domain.Models;
using EtaiEcoSystem.EventBus.Models.DTOs.Base;
using Microsoft.AspNetCore.Mvc;
using TaskDistribution.BLL.Interfaces;

namespace TaskDistribution.API.Controllers
{
    [Route("api/workspaces/{WorkspaceId}/projects/{ProjectId}/boards/{BoardId}/distribution")]
    public class DistributionController : BaseController
    {
        #region Injects

        private readonly IBusinessManager _bll;

        #endregion

        #region Ctors

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="bll">Точка доступа к BLL</param>
        public DistributionController(IBusinessManager bll)
        {
            _bll = bll;
        }

        #endregion

        [HttpPost]
        public Task ActionRequest(BaseBoardRequest request, CancellationToken ctn) =>
            _bll.Distribution.DistributionTask(request, ctn);
    }
}
