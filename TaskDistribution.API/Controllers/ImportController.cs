using Common.Requests;
using EtaiEcoSystem.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using TaskDistribution.BLL.Interfaces;

namespace TaskDistribution.API.Controllers
{
    [Route("api/workspaces/{WorkspaceId}/projects/{ProjectId}/boards/{BoardId}/distribution/import")]
    public class ImportController : BaseController
    {
        #region Injects

        private readonly IBusinessManager _bll;

        #endregion

        #region Ctors

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="bll">Точка доступа к BLL</param>
        public ImportController(IBusinessManager bll)
        {
            _bll = bll;
        }

        #endregion

        [HttpPost]
        public Task ActionRequest(ImportRequest request, CancellationToken ctn) =>
            _bll.ImportService.Import(request, ctn);
    }
}
