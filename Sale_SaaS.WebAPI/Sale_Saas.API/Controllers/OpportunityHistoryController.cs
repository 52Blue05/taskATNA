using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.ContractFeature.Commands;
using Sale_Saas.Application.Features.ContractStatusFeature.Queries;
using Sale_Saas.Application.Features.CustomerFeature.Queries;
using Sale_Saas.Application.Features.OpportunityFeature.Queries;
using Sale_Saas.Application.Features.OpportunityHistoryFeature.Commands;
using Sale_Saas.Application.Features.OpportunityHistoryFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Interfaces.Services.Identity;
using Sale_Saas.Application.Models.Opportunity;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpportunityHistoryController : BaseController
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILoggerService _loggerService;

        public OpportunityHistoryController(IApplicationUserService applicationUserService, ILoggerService loggerService)
        {
            _applicationUserService = applicationUserService;
            _loggerService = loggerService;
        }

        [HttpGet("get-by-id/{id}")]
        [HasPermission(PolicyTypes.Sale_CH.CH_XEMCAPNHAT)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new OpportunityHistory_GetByIdQuery(id)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("add-or-update")]
		[HasPermission(PolicyTypes.Sale_CH.CH_THEMCAPNHAT)]
		public async Task<IActionResult> AddOrUpdate([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new OpportunityHistory_AddOrUpdateCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
		[HasPermission(PolicyTypes.Sale_CH.CH_THEMCAPNHAT)]
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

                return Ok(await Mediator.Send(new OpportunityHistory_DeleteByIdCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("get-list-with-pagination")]
		[HasPermission(PolicyTypes.Sale_CH.CH_XEMCAPNHAT)]
		public async Task<IActionResult> GetListWithPagination([FromQuery] OpportunityGetByIdRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                request.UserId = GetCurrentUser();
                return Ok(await Mediator.Send(new OpportunityHistory_GetListWithPaginationQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

		[HttpPost("import")]
		public async Task<IActionResult> Import(IFormFile file)
		{
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<OpportunityHistory> listOpportunityHistories = new List<OpportunityHistory>();
                var loginUserId = GetCurrentUser();
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowcount = worksheet.Dimension.Rows;
                        var columnCount = worksheet.Dimension.Columns;
                        for (int row = 2; row <= rowcount; row++)
                        {
                            var empOpHistory = new OpportunityHistory();
                            empOpHistory.Id = Guid.NewGuid();
                            empOpHistory.DeleteFlag = false;
                            empOpHistory.CreatedDate = DateTime.Now;
                            empOpHistory.LastModifiedDate = DateTime.Now;
                            empOpHistory.CreatedApplicationUserId = loginUserId;
                            empOpHistory.LastModifiedApplicationUserId = loginUserId;
                            for (int column = 1; column <= columnCount; column++)
                            {
                                if (worksheet.Cells[row, column].Value.ToString() != null)
                                {
                                    switch (worksheet.Cells[1, column].Value.ToString())
                                    {
                                        case "Email của nhân viên":
                                            var userId = await _applicationUserService.GetUserIdByEmail(worksheet.Cells[row, column].Value.ToString() ?? "");
                                            if (userId.Data == Guid.Empty)
                                                return Ok(Result<string>.Failure($"Không tìm thấy nhân viên với email: {worksheet.Cells[row, column].Value.ToString()}"));
                                            empOpHistory.ApplicationUserId = userId.Data;
                                            break;
                                        case "Mục tiêu":
                                            empOpHistory.Goal = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Hoạt động":
                                            empOpHistory.Activity = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Thời gian":
                                            empOpHistory.Time = DateTime.Parse(worksheet.Cells[row, column].Value.ToString() ?? DateTime.Now.ToString());
                                            break;
                                        case "Kết quả":
                                            empOpHistory.Result = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Mã cơ hội":
                                            empOpHistory.OpportunityId = Guid.Parse(worksheet.Cells[row, column].Value.ToString() ?? Guid.Empty.ToString());
                                            break;
                                    }
                                }
                            }
                            listOpportunityHistories.Add(empOpHistory);
                        }
                    }
                }
                if (listOpportunityHistories.Count > 0)
                {
                    return Ok(await Mediator.Send(new OpportunityHistory_ImportCommand(listOpportunityHistories)));
                }

                return Ok(Result<string>.Failure("File import khác file mẫu!"));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

		[HttpGet("export")]
		public async Task<IActionResult> Export([FromQuery] OpportunityGetByIdRequest request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				request.UserId = GetCurrentUser();
				var response = await Mediator.Send(new OpportunityHistory_GetListWithPaginationQuery(request));
				if (response.Succeeded == false || response.Data == null)
					throw new ApplicationException("Xuất file excel thất bại!");
				var data = response.Data.Items;
				ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
				ExcelPackage excel = new ExcelPackage();
                string locale = GetLocale();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách lịch sử cập nhật cơ hội" : "List Opportunitis history";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
				workSheet = ExcelExportHelper.GetStyle(workSheet, 5);

				workSheet.Cells[1, 1].Value = "NGƯỜI CẬP NHẬT";
				workSheet.Cells[1, 2].Value = "MỤC TIÊU";
				workSheet.Cells[1, 3].Value = "HOẠT ĐỘNG";
				workSheet.Cells[1, 4].Value = "THỜI ĐIẺM";
				workSheet.Cells[1, 5].Value = "KẾT QUẢ";

				int currRow = 2;

				foreach (var item in data)
				{
					workSheet.Row(currRow).Height = 20;
					workSheet.Cells[currRow, 1].Value = item.ApplicationUser;
					workSheet.Cells[currRow, 2].Value = item.Goal;
					workSheet.Cells[currRow, 3].Value = item.Activity;
					workSheet.Cells[currRow, 4].Value = item.Time.HasValue ? item.Time.Value.ToString("dd-mm-yyyy") : "";
					workSheet.Cells[currRow, 5].Value = item.Result;

					currRow++;
				}

				workSheet.Cells.AutoFitColumns();

				return File(excel.GetAsByteArray(), "application/vnd.ms-excel", String.Format("{0}.xlsx", listName));
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}
	}
}
