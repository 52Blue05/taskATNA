using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Infrastructure.Services.TenantService;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrackingLogController : BaseController
    {
        private readonly ITrackingLogService _trackingLogService;
        private readonly ILoggerService _loggerService;

        public TrackingLogController(ITrackingLogService trackingLogService, ILoggerService loggerService)
        {
            _trackingLogService = trackingLogService;
            _loggerService = loggerService;
        }

        [HttpGet("get-list-all-request")]
        public async Task<IActionResult> GetListAllRequest([FromQuery] TrackingLogRequest request)
        {
            try
            {
                var data = await _trackingLogService.GetListAllAverageRequest(request);
                return Ok(Result<List<TrackingLogDto>>.Success(data.Data));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-error-request")]
        public async Task<IActionResult> GetListErrorRequest([FromQuery] TrackingLogRequest request)
        {
            try
            {
                var data = await _trackingLogService.GetListErrorAverageRequest(request);
                return Ok(Result<List<TrackingLogDto>>.Success(data.Data));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-downtime-request")]
        public async Task<IActionResult> GetListDowntimeRequest([FromQuery] TrackingLogRequest request)
        {
            try
            {
                var data = await _trackingLogService.GetListDowntimeAverageRequest(request);
                return Ok(Result<List<TrackingLogDto>>.Success(data.Data));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-list-detail-by-time")]
        public async Task<IActionResult> GetListDetailByTime([FromQuery] TrackingLogRequest request)
        {
            try
            {
                var data = await _trackingLogService.GetListDetailByTime(request);
                return Ok(Result<List<TrackingLogAverageResult>>.Success(data.Data));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
