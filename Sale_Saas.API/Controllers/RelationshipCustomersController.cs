using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.RelationshipCustomerFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
     [Route("api/[controller]")]
     [ApiController]
     public class RelationshipCustomersController : BaseController
     {
          private readonly IApplicationUserService _applicationUserService;
          private readonly ILoggerService _loggerService;

          public RelationshipCustomersController(IApplicationUserService applicationUserService, ILoggerService loggerService)
          {
               _applicationUserService = applicationUserService;
               _loggerService = loggerService;
          }

          [HttpGet("get-all")]
          [HasPermission(PolicyTypes.Sale_MQH.View)]
          public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
          {
               try
               {
                    if (!ModelState.IsValid)
                    {
                         return BadRequest(ModelState);
                    }

                    return Ok(await Mediator.Send(new RelationshipCustomer_GetAllQuery(request)));
               }
               catch (Exception ex)
               {
                    _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                    return Ok(Result<string>.Failure(ex.Message));
               }
          }
     }
}
