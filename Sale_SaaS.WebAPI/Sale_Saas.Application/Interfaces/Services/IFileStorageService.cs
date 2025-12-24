using Sale_Saas.Application.Models.Media;
using Sale_Saas.Application.Models.Storage;

namespace Sale_Saas.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task SaveFileAsync(Stream mediaBinaryStream, string fileName);
    Task DeleteFileAsync(string fileName);
    Task<List<MinIOCloudModel>> UploadFileAsync(List<IFormFile> files, string folderName = "sass", bool? isGroupTenant = false);
    Task<MediaResponseDto> DownloadFileAsync(string fileName, string folderName = "sass");
    Task DeleteMediaAsync(string fileName, string folderName = "sass");
}
