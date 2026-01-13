using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.SaleKitFeature.Commands;
using Sale_Saas.Application.Features.SaleKitFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Models.Role_SaleKit;
using Sale_Saas.Application.Models.SaleKit;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleKitController : BaseController
    {
        private readonly ILoggerService _loggerService;

        public SaleKitController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpPost("add-document")]
        //[HasPermission(PolicyTypes.Sale_SK.CREATE)]
        public async Task<IActionResult> AddOrUpdate([FromForm] SaleKitAddRequestController data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var request = new SaleKitAddRequest()
                {
                    files = data.Files,
                    ApplicationUserId = GetCurrentUser(),
                    Folder = GetCurrentTenant() == "" ? "sass" : GetCurrentTenant(),
                    ParentId = data.ParentId,
                    Name = data.Name,
                    Type = data.Type
                };
                var userId = GetCurrentUser() ?? Guid.Empty;
                return Ok(await Mediator.Send(new SaleKit_AddOrUpdateCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("download-document")]
        public async Task<IActionResult> DownloadDocument(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                SaleKitDownloadRequest request = new SaleKitDownloadRequest()
                {
                    Id = id,
                    UserId = GetCurrentUser() ?? Guid.Empty
                };
                //var response = await Mediator.Send(new SaleKit_DownloadByIdCommand(request));
                //return File(response.Bytes, response.SaleKit.Extension!);
                return Ok(await Mediator.Send(new SaleKit_DownloadByIdCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("filter")]
        [HasPermission(PolicyTypes.Sale_SK.VIEW)]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new SaleKit_GetListQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-all")]
        [HasPermission(PolicyTypes.Sale_SK.VIEW)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new SaleKit_GetAllQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("{Id}/get-parent")]
        public async Task<IActionResult> GetParent([FromRoute] Guid Id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new SaleKit_GetParentQuery(Id)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-list-with-pagination")]
        public async Task<IActionResult> GetListWithPagination([FromQuery] SaleKitGetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                request.UserId = GetCurrentUser();
                return Ok(await Mediator.Send(new SaleKit_GetListWithPaginationQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id/{id}")]
        [HasPermission(PolicyTypes.Sale_SK.VIEW)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new SaleKit_GetByIdQuery(id)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
        //[HasPermission(PolicyTypes.Sale_SK.DELETE)]
        public async Task<IActionResult> DeleteByIds(string ids, Guid ApplicationUserId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var request = new DeleteRequest()
                {
                    Ids = ids.Split(",").ToList(),
                    ApplicationUserId = ApplicationUserId
                };

                return Ok(await Mediator.Send(new SaleKit_DeleteByIdCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-list-by-current-user")]
        public async Task<IActionResult> GetListByCurrentUserByMobile([FromQuery] RoleSaleKitGetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new SaleKitMobile_GetListWithPaginationByRoleQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("mobile/get-list-by-current-user/{Id}")]
        public async Task<IActionResult> GetListByFolder([FromQuery] RoleSaleKitGetListWithPaginationQueryRequest request, Guid Id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.ParentId = Id;
                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new SaleKitMobile_GetListWithPaginationByRoleQuery(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
