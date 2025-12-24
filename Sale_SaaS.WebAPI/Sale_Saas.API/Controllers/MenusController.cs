using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.MenuFeature.Commands;
using Sale_Saas.Application.Features.MenuFeature.Dto;
using Sale_Saas.Application.Features.MenuFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
     public class MenusController : BaseController
     {
          private readonly ILoggerService _loggerService;

          public MenusController(ILoggerService loggerService)
          {
               _loggerService = loggerService;
          }

          [HttpGet("filter")]
          public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
          {
               try
               {
                    if (!ModelState.IsValid)
                    {
                         return BadRequest(ModelState);
                    }

                    return Ok(await Mediator.Send(new Menu_GetListQuery(request)));
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

                    return Ok(await Mediator.Send(new Menu_GetAllQuery(request)));
               }
               catch (Exception ex)
               {
                    _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                    return Ok(Result<string>.Failure(ex.Message));
               }
          }
          [HttpGet("get-by-user")]
          public async Task<IActionResult> GetByUser()
          {
               try
               {
                    if (!ModelState.IsValid)
                    {
                         return BadRequest(ModelState);
                    }

                    string adminAta = GetCurrentUserAdmin() == null ? "NoAdminAta" : GetCurrentUserAdmin();

                    return Ok(await Mediator.Send(new Menu_GetByUserQuery(GetCurrentUser() ?? Guid.Empty, adminAta)));
               }
               catch (Exception ex)
               {
                    _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod().Name);
                    return Ok(Result<string>.Failure(ex.Message));
               }
          }
          [HttpGet("get-list-with-pagination")]
          public async Task<IActionResult> GetListWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
          {
               try
               {
                    if (!ModelState.IsValid)
                    {
                         return BadRequest(ModelState);
                    }

                    return Ok(await Mediator.Send(new Menu_GetListWithPaginationQuery(request)));
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

                    return Ok(await Mediator.Send(new Menu_GetByIdQuery(id)));
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

                    return Ok(await Mediator.Send(new Menu_AddOrUpdateCommand(request)));
               }
               catch (Exception ex)
               {
                    _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                    return Ok(Result<string>.Failure(ex.Message));
               }
          }

          /*[HttpPost("init-feature")]
		public async Task<IActionResult> InitFeature()
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				return Ok(await Mediator.Send(new Menu_InitFeatureCommand()));
			}
			catch (Exception ex)
			{
				return Ok(Result<string>.Failure(ex.Message));
			}
		}*/

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

                    return Ok(await Mediator.Send(new Menu_DeleteByIdCommand(request)));
               }
               catch (Exception ex)
               {
                    _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                    return Ok(Result<string>.Failure(ex.Message));
               }
          }

          [HttpPost("update-active")]
          public async Task<IActionResult> UpdateActive([FromBody] MenuUpdateActiveDto request)
          {
               try
               {
                    if (!ModelState.IsValid)
                    {
                         return BadRequest(ModelState);
                    }

                    return Ok(await Mediator.Send(new Menu_UpdateActiveCommand(request)));
               }
               catch (Exception ex)
               {
                    _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                    return Ok(Result<string>.Failure(ex.Message));
               }
          }
     }
}
