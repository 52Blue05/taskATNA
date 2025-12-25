using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.ResultFeature.Commands;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Models.ResultExam;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultController : BaseController
    {
        private readonly ILoggerService _loggerService;

        public ResultController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpPost("create-result-exam")]
        public async Task<IActionResult> CreateResultExam([FromBody] ResultExamRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Result_CaculateResultOfExamCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
