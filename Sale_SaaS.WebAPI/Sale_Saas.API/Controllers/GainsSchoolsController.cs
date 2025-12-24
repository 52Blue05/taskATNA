using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.GainsSchoolFeature.Commads;
using Sale_Saas.Application.Interfaces.Services;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
     [Route("api/[controller]")]
     [ApiController]
     public class GainsSchoolsController : BaseController
     {
          private readonly ILoggerService _loggerService;

          public GainsSchoolsController(ILoggerService loggerService)
          {
               _loggerService = loggerService;
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

                    return Ok(await Mediator.Send(new GainsSchool_AddOrUpdateCommand(request)));
               }
               catch (Exception ex)
               {
                    _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                    return Ok(Result<string>.Failure(ex.Message));
               }
          }
     }
}
