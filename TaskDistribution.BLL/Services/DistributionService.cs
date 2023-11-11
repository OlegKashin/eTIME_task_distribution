using EtaiEcoSystem.Domain.Enums;
using EtaiEcoSystem.EventBus.Models.DTOs.Base;
using EtaiEcoSystem.EventBus.Models.DTOs.TaskManager.Requests.BoardRequests;
using EtaiEcoSystem.EventBus.Models.DTOs.TaskManager.Requests.CardRequests;
using EtaiEcoSystem.EventBus.Models.DTOs.TaskManager.Responses.BoardResponses;
using EtaiEcoSystem.EventBus.Models.DTOs.TaskManager.Responses.CardResponses;
using EtaiEcoSystem.EventBus.Models.DTOs.Workspace.Requests;
using EtaiEcoSystem.EventBus.Models.DTOs.Workspace.Requests.ItemPermissionRequests;
using EtaiEcoSystem.EventBus.Models.DTOs.Workspace.Responses;
using EtaiEcoSystem.EventBus.Models.DTOs.Workspace.Responses.RoleResponses;
using EtaiEcoSystem.EventBus.Models.SharedEntities.TaskManager;
using EtaiEcoSystem.PermissionsMachining;
using FluentChanger.Json.Extension;
using Integration.Google.Maps.Models.Response;
using Microsoft.AspNetCore.Cors.Infrastructure;
using System.Drawing;
using TaskDistribution.BLL.Interfaces;
using TaskDistribution.BLL.Models;
using Task = TaskDistribution.BLL.Models.Task;
using Thread = System.Threading.Tasks.Task;

namespace TaskDistribution.BLL.Services
{
    internal class DistributionService : IDistributionService
    {
        private readonly BusinessManager _bll;

        public DistributionService(BusinessManager bll)
        {
            _bll = bll;
        }

        public async Thread DistributionTask(BaseBoardRequest request,CancellationToken ctn = default) 
        {
            var cardsResponse = await _bll.BusManager.SendAsync<GetCardsRequest,GetCardsResponse>(new GetCardsRequest 
            {
                AuthorizedUserId = request.AuthorizedUserId,
                WorkspaceId = request.WorkspaceId,
                ProjectId = request.ProjectId,
                BoardId = request.BoardId,
                CardFilters = new CardFilters 
                {
                    ActiveSprint = false,
                    NoExecutors = true
                }
            });

            var cards = cardsResponse.Message.Cards.Where(x => x.Priority > 0);

            var rolesMessage = await _bll.BusManager.SendAsync<GetItemRolesRequest, GetItemRolesResponse>(new GetItemRolesRequest
            {
                AuthorizedUserId =  request.AuthorizedUserId,
                WorkspaceId = request.WorkspaceId,
                ItemId = request.WorkspaceId,
                TypeId = WorkspaceItemType.Workspace,
            });

            var userRoles = rolesMessage.Message.Roles
                .Where(x => x.Type == (short)RoleType.Custom)
                .OrderBy(x => x.Order)
                .Take(3)
                .Select((role,i)=> new { role.Id, Index = i+1});

            var addresses = cards.DistinctBy(x=>x.Title).Select(x => x.Title).ToList();

            string[] taskAddressesGeocode = await Thread.WhenAll(addresses.Select(address => _bll.GoogleApi.GeocodeAddress(address, ctn)));

            var addressesGeocodes = taskAddressesGeocode.Select((geocode,i) => new { address = addresses.ElementAt(i), geocode });

            var usersResponse = await _bll.BusManager.SendAsync<GetWorkspaceRequest, GetWorkspaceResponse>(new GetWorkspaceRequest
            {
                AuthorizedUserId= request.AuthorizedUserId,
                WorkspaceId = request.WorkspaceId,
            });

            var users = usersResponse.Message.Users.Where(user => user.Roles.Any(x => userRoles.Any(role => role.Id == x)));

            var usersAddresses = users.DistinctBy(x => x.OfficeAddress).Select(x => x.OfficeAddress).ToList();

            var usersAddressesGeocode = await Thread.WhenAll(usersAddresses.Select(address => _bll.GoogleApi.GeocodeAddress(address, ctn)));

            var usersGeocodes = usersAddressesGeocode.Select((geocode, i) => (address: usersAddresses.ElementAt(i), geocode: geocode));

            var mapElements = await _bll.GoogleApi.CalculateDistances(usersAddressesGeocode.Concat(taskAddressesGeocode).ToArray(), taskAddressesGeocode);

            var usersAddressesCount = usersAddresses.Count();

            var userMapElements = mapElements[0..usersAddressesCount]
                .Join(usersGeocodes, x => x.Address, x => x.geocode, (point, address) => new { point, address }).ToList();

            var taskMapElementsAddress = mapElements[usersAddressesCount..]
                .Join(addressesGeocodes, x => x.Address, x => x.geocode, (point, address) => new { point, address }).ToList();

            var taskMapElements = cards
                .Join(taskMapElementsAddress, x => x.Title, x =>  x.address.address, (card, point) => new { point.point, card,
                    task = new Task {
                        Id = card.CardId,
                        Priority = (int)card.Priority,
                        Time = card.EstimateTime!.Value * 60 // переводим в секунды
                    }
                }).ToList();

            var executors = users
                .Join(userMapElements, x => x.OfficeAddress, x => x.address.address, (user,map)=> new {user, map.point, map.address})
                .Select(item=> {
                    var roleMap = userRoles.First(x => x.Id == item.user.Roles.OrderBy(x => x).ElementAt(1));
                    return new { item.point, roleMap, executor = new Executor(item.user.Id, roleMap.Index, 32400) };
                })
                .ToList();

            var result = executors.ToDictionary(x=>x.executor.Id , x => new List<Tuple<CardModel, MapElement?>>());
            int tmpCountPoint = 0;
            do
            {
                tmpCountPoint = result.Sum(x => x.Value.Count());
                foreach (var executor in executors)
                {
                    if (executor.executor.WorkTime < 5400)
                        continue;

                    var cardsResult = result[executor.executor.Id];

                    MapElement currentPoint = cardsResult.LastOrDefault()?.Item2 ?? executor.point;

                    var tt = taskMapElements.Where(task => currentPoint.Roads.Any(road => task.point.Address == road.Address)).ToList();

                    var nextPoints = tt
                        .Join(currentPoint.Roads, x => x.point.Address, x => x.Address, (item, road) => new CardPoint(item.task, item.card, item.point, road.Duration))
                        .ToList();

                    CardPoint nextPoint = null!;
                    var maxWeight = 0;
                    var minDuration = int.MaxValue;
                    foreach (var point in nextPoints)
                    {
                        var vertex = new Vertex(point.task, executor.executor);
                        if (vertex.Weight > 0 && vertex.Weight >= maxWeight && point.duration < minDuration && executor.executor.WorkTime - (point.duration + point.task.Time) >= 0)
                        {
                            maxWeight = vertex.Weight;
                            minDuration = point.duration;
                            nextPoint = point;
                        }
                    }

                    if (nextPoint == null)
                        continue;

                    taskMapElements.Remove(taskMapElements.First(x => x.card.CardId == nextPoint.card.CardId));
                    cardsResult.Add(new Tuple<CardModel, MapElement?>(nextPoint.card, nextPoint.mapElement));
                    executor.executor.SetWorkTime(nextPoint.duration + nextPoint.task.Time);
                }

            } while (taskMapElements.Count != 0 && tmpCountPoint != result.Sum(x => x.Value.Count()));

            await Thread.WhenAll(result.SelectMany(item => item.Value.Select(card => _bll.BusManager.SendAndForgetAsync(new AttachExecutorUserToCardRequest
            {
                AuthorizedUserId = request.AuthorizedUserId,
                WorkspaceId = card.Item1.WorkspaceId,
                ProjectId = card.Item1.ProjectId,
                BoardId = card.Item1.BoardId,
                CardId = card.Item1.CardId,
                ExecutorUserId = item.Key
            }, ctn))));


            var boardResponseMessage = await _bll.BusManager.SendAsync<GetBoardRequest, GetBoardResponse>(new GetBoardRequest
            {
                AuthorizedUserId = request.AuthorizedUserId,
                WorkspaceId = request.WorkspaceId,
                ProjectId = request.ProjectId,
                BoardId = request.BoardId,
            },ctn);

            await Thread.WhenAll(result.SelectMany(item => item.Value.Select((card,i) => 
            {
                var dataUpdate = new JsonChangerUpdate<UpdateCardRequestData>();
                dataUpdate.Update(x => x.DueDate, DateTime.UtcNow);
                dataUpdate.Update(x => x.SprintId, boardResponseMessage.Message.Board.ActiveSprintId);
                dataUpdate.Update(x => x.Order, i);
                return _bll.BusManager.SendAndForgetAsync(new UpdateCardRequest
                {
                    AuthorizedUserId = request.AuthorizedUserId,
                    WorkspaceId = card.Item1.WorkspaceId,
                    ProjectId = card.Item1.ProjectId,
                    BoardId = card.Item1.BoardId,
                    CardId = card.Item1.CardId,
                    Data = dataUpdate
                }, ctn);
            })));
        }
        private record CardPoint(Task task, CardModel card, MapElement mapElement, int duration);
    }
}
