using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.BenefitFeature.Queries;
using Sale_Saas.Application.Features.GoalFeature.Dto;
using Sale_Saas.Application.Features.GoalFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Notification;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;

namespace Sale_Saas.API.Controllers.Tenant
{
	[Route("api/[controller]")]
	[ApiController]
	//[Authorize]
	public class NotificationController : BaseController
	{
		private readonly INotificationService _notificationService;
		private readonly ILoggerService _loggerService;

		public NotificationController(INotificationService notificationService, ILoggerService loggerService)
		{
			_notificationService = notificationService;
			_loggerService = loggerService;
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

                var userId = GetCurrentUser() ?? Guid.Empty;

                var response = Result<NotificationDto>.Success(await _notificationService.GetById(id, userId));
				return Ok(response);
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
				request.UserId = GetCurrentUser();
				return Ok(await _notificationService.GeListWithPaginationQuery(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
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

				request.UserId = GetCurrentUser();

				return Ok(await _notificationService.GetListQuery(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPost("push")]
		public async Task<IActionResult> Push([FromBody] List<PushNotificationRequest> request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var user = GetCurrentUser() ?? Guid.Empty;
				var response = Result<List<NotificationDto>>.Success(await _notificationService.PushAsync(request));
				return Ok(response);
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpGet("get-count-unseen")]
		public async Task<IActionResult> Unseen()
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				var user = GetCurrentUser() ?? Guid.Empty;

				var response = Result<int>.Success(await _notificationService.CountUnseenAsync(user));
				return Ok(response);
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
				return Ok(await _notificationService.DeleteByIds(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPut("read-message/{ids}/{ApplicationUserId}")]
		public async Task<IActionResult> ReadByIds(string ids, Guid ApplicationUserId)
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
				return Ok(await _notificationService.ReadMessageAsync(request));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

		[HttpPut("read-all/{ApplicationUserId}")]
		public async Task<IActionResult> ReadAll(Guid ApplicationUserId)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				return Ok(await _notificationService.ReadAllMessageAsync(ApplicationUserId));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
			}
		}

        [HttpGet("mobile/get-count-unseen")]
        public async Task<IActionResult> MobileUnseen()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var user = GetCurrentUser() ?? Guid.Empty;

                var response = Result<int>.Success(await _notificationService.CountUnseenAsync(user));
                return Ok(response);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPut("mobile/detail/{ids}")]
        public async Task<IActionResult> MobileReadByIds(string ids)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var user = GetCurrentUser() ?? Guid.Empty;
                var request = new DeleteRequest()
                {
                    Ids = ids.Split(",").ToList(),
                    ApplicationUserId = user
                };
                return Ok(await _notificationService.ReadMessageByMobile(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("mobile/list")]
        public async Task<IActionResult> GetList([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                request.UserId = GetCurrentUser();

                return Ok(await _notificationService.GetListQueryByMobile(request));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
    }
}
