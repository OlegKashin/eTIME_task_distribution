using EtaiEcoSystem.EventBus.Models.DTOs.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Common.Requests
{
    public record ImportRequest : BaseBoardRequest
    {
        [FromForm] public required IFormFile File { get; init; }
    }
}
