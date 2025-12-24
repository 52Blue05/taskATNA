using Microsoft.AspNetCore.Mvc;
using Sale_Saas.Application.Common.Models;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Models.Storage;
using Sale_Saas.Application.Utilities;
using System.Reflection;
namespace Sale_Saas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StorageController : BaseController
    {
        private readonly IFileStorageService _StorageService;
        private readonly ILoggerService _loggerService;

        public StorageController(IFileStorageService StorageService, ILoggerService loggerService)
        {
            _StorageService = StorageService;
            _loggerService = loggerService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(List<IFormFile> files,string folderName = "sass")
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var tenant = GetCurrentTenant();
                if (string.IsNullOrEmpty(tenant))
                    throw new ApplicationException("Tài khoản không có quyền thao tác");
                folderName = tenant;
                var response = await _StorageService.UploadFileAsync(files,folderName);
                return Ok(Result<List<MinIOCloudModel>>.Success(response));
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }
        [HttpGet("download")]
        public async Task<IActionResult> Download(string fileName, string folderName = "sass")
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
				var result = await _StorageService.DownloadFileAsync(fileName, folderName);
                return File(result.data, result.Extension, fileName);
            }
            catch (Exception ex)
            {
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
            }
        }

		[HttpGet("{folder}/{name}")]
		public async Task<IActionResult> Image([FromRoute] string folder,[FromRoute] string name)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}
				var result = await _StorageService.DownloadFileAsync(name, folder);
				return File(result.data, result.Extension);
			}
			catch (Exception ex)
			{
                _loggerService.WriteErrorLog(ex, functionName: MethodBase.GetCurrentMethod()?.Name ?? "UnknownMethod");
                return Ok(Result<string>.Failure(ex.Message));
			}
		}
    }
}
