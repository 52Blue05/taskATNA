using Microsoft.AspNetCore.Mvc;
using Minio.DataModel;
using OfficeOpenXml;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Features.CustomerFeature.Commands;
using Sale_Saas.Application.Features.CustomerFeature.Queries;
using Sale_Saas.Application.Features.SupplierFeature.Commands;
using Sale_Saas.Application.Features.SupplierFeature.Queries;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Domain.Entities;
using Sale_Saas.Domain.Enums;
using Sale_Saas.Infrastructure.Authentication;
using System.Reflection;

namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : BaseController
    {
        private readonly ILoggerService _loggerService;

        public SupplierController(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        [HttpPost("filter")]
        [HasPermission(PolicyTypes.DM_NCC.VIEW)]
        public async Task<IActionResult> Filter([FromQuery] FilterQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Supplier_GetListQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("get-all")]
        [HasPermission(PolicyTypes.DM_NCC.VIEW)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Supplier_GetAllQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpPost("get-list-with-pagination")]
        [HasPermission(PolicyTypes.DM_NCC.VIEW)]
        public async Task<IActionResult> GetListWithPagination([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Supplier_GetListWithPaginationQuery(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpGet("get-by-id/{id}")]
        [HasPermission(PolicyTypes.DM_NCC.VIEW)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                return Ok(await Mediator.Send(new Supplier_GetByIdQuery(id)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("add-or-update")]
        //[HasPermission(PolicyTypes.DM_NCC.CREATE)]
        public async Task<IActionResult> AddOrUpdate([FromBody] List<AddOrUpdateRequest> request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId =  GetCurrentUser() ?? Guid.Empty;

                return Ok(await Mediator.Send(new Supplier_AddOrUpdateCommand(userId, request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpDelete("delete-by-ids/{ids}/{ApplicationUserId}")]
        [HasPermission(PolicyTypes.DM_NCC.DELETE)]
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

                return Ok(await Mediator.Send(new Supplier_DeleteByIdCommand(request)));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");

                return Ok(Result<string>.Failure(ex.Message));
            }
        }

        [HttpPost("import")]
        [HasPermission(PolicyTypes.DM_NCC.IMPORT_EXCEL)]
        public async Task<IActionResult> Import(IFormFile file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                List<Supplier> listSuppliers = new List<Supplier>();
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
                            var empsupplier = new Supplier();
                            empsupplier.Id = Guid.NewGuid();
                            empsupplier.DeleteFlag = false;
                            empsupplier.CreatedDate = DateTime.Now;
                            empsupplier.LastModifiedDate = DateTime.Now;
                            empsupplier.CreatedApplicationUserId = loginUserId;
                            empsupplier.LastModifiedApplicationUserId = loginUserId;
                            for (int column = 1; column <= columnCount; column++)
                            {
                                if (worksheet.Cells[row, column].Value.ToString() != null)
                                {
                                    switch (worksheet.Cells[1, column].Value.ToString())
                                    {
                                        case "Mã NCC":
                                            empsupplier.Code = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Tên NCC":
                                            empsupplier.Name = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Mô tả":
                                            empsupplier.Description = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                        case "Đánh giá":
                                            empsupplier.Review = worksheet.Cells[row, column].Value.ToString();
                                            break;
                                    }
                                }
                            }
                            listSuppliers.Add(empsupplier);
                        }
                    }
                }
                if (listSuppliers.Count > 0)
                {
                    return Ok(await Mediator.Send(new Supplier_ImportCommand(listSuppliers)));
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
        [HasPermission(PolicyTypes.DM_NCC.EXPORT_EXCEL)]
        public async Task<IActionResult> Export([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                string locale = GetLocale();
                request.UserId = GetCurrentUser();
                var response = await Mediator.Send(new Supplier_GetListWithPaginationQuery(request));
                if (response.Succeeded == false || response.Data == null)
                    throw new ApplicationException("Xuất file excel thất bại!");
                var data = response.Data.Items;
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = locale == LocaleEnum.vi_VN.ToString() ? "Danh sách nhà cung cấp" : "List suppliers";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 5);

                workSheet.Cells[1, 1].Value = "ID";
                workSheet.Cells[1, 2].Value = "MÃ NCC";
                workSheet.Cells[1, 3].Value = "TÊN NCC";
                workSheet.Cells[1, 4].Value = "MÔ TẢ";
                workSheet.Cells[1, 5].Value = "ĐÁNH GIÁ";

                if (locale == LocaleEnum.en_US.ToString())
                {
                    workSheet.Cells[1, 2].Value = "SUPPLIER CODE";
                    workSheet.Cells[1, 3].Value = "CUSTOMER FULL NAME";
                    workSheet.Cells[1, 4].Value = "DESCRIPTION";
                    workSheet.Cells[1, 5].Value = "REVIEW";
                }

                int currRow = 2;

                foreach (var item in data)
                {
                    workSheet.Row(currRow).Height = 20;
                    workSheet.Cells[currRow, 1].Value = item.Id;
                    workSheet.Cells[currRow, 2].Value = item.Code;
                    workSheet.Cells[currRow, 3].Value = item.Name;
                    workSheet.Cells[currRow, 4].Value = item.Description;
                    workSheet.Cells[currRow, 5].Value = item.Review;

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

        [HttpGet("export-sample")]
        [HasPermission(PolicyTypes.DM_NCC.EXPORT_EXCEL)]
        public async Task<IActionResult> ExportSample([FromQuery] GetListWithPaginationQueryRequest request)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                ExcelPackage excel = new ExcelPackage();
                string listName = "File import mẫu của nhà cung cấp";
                var workSheet = excel.Workbook.Worksheets.Add(listName);
                workSheet = ExcelExportHelper.GetStyle(workSheet, 4);

                workSheet.Cells[1, 1].Value = "Mã NCC";
                workSheet.Cells[1, 2].Value = "Tên NCC";
                workSheet.Cells[1, 3].Value = "Mô tả";
                workSheet.Cells[1, 4].Value = "Đánh giá";

                workSheet.Row(2).Height = 20;
                workSheet.Cells[2, 1].Value = "AUD";
                workSheet.Cells[2, 2].Value = "Sauer, Feil and Mraz";
                workSheet.Cells[2, 3].Value = "Lead";
                workSheet.Cells[2, 4].Value = "The Nagasaki Lander is the trademarked \"Name\" of several series of Nagasaki sport bikes, that started with the 1984 ABC800J";

                workSheet.Row(3).Height = 20;
                workSheet.Cells[3, 1].Value = "TWD";
                workSheet.Cells[3, 2].Value = "Bahringer - Friesen";
                workSheet.Cells[3, 3].Value = "Direct";
                workSheet.Cells[3, 4].Value = "The beautiful range of Apple Naturalé that has an exciting mix of natural ingredients. With the Goodness of 100% Natural Ingredients";

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
