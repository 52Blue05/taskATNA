using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.OrganizationFeature.Commands;
using Sale_Saas.Application.Features.OrganizationFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : BaseController
    {
        private readonly ILoggerService _loggerService;

        public OrganizationController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpPost("filter")]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Organization_GetListQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Organization_GetAllQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("get-list-with-pagination")]
        public async Task<IActionResult> GetListWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Organization_GetListWithPaginationQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Organization_GetByIdQuery(id)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("add-or-update")]
        public async Task<IActionResult> AddOrUpdate([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Organization_AddOrUpdateCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
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

                return Ok(await Mediator.Send(new Organization_DeleteByIdCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
