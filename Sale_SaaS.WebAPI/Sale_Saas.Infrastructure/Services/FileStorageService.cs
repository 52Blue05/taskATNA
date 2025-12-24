using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Sale_Saas.Application.Interfaces.Services;
using Sale_Saas.Application.Models.Storage;
using System.Drawing.Imaging;
using System.Drawing;
using Sale_Saas.Application.Utilities;
using Sale_Saas.Application.Common.Interfaces;
using Sale_Saas.Application.Models.Media;

namespace Sale_Saas.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
	private readonly CancellationToken _cancellationToken = new CancellationToken();
	private readonly string _userContentFolder;
    private readonly IMinioClient _minioClient;
	private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApplicationDbContext _context;
    
	public FileStorageService(
        IWebHostEnvironment webHostEnvironment, 
        IMinioClient minioClient, 
        IHttpContextAccessor httpContextAccessor,
        IApplicationDbContext context)
    {
        _userContentFolder = webHostEnvironment.WebRootPath;
        _minioClient = minioClient;
		_httpContextAccessor = httpContextAccessor;
        _context = context;
	}

    public async Task SaveFileAsync(Stream mediaBinaryStream, string fileName)
    {
        var filePath = _userContentFolder + fileName;
        using var output = new FileStream(filePath, FileMode.Create);
        await mediaBinaryStream.CopyToAsync(output);
    }

    public async Task DeleteFileAsync(string fileName)
    {
        var filePath = Path.Combine(_userContentFolder, fileName);
        if (File.Exists(filePath))
        {
            await Task.Run(() => File.Delete(filePath));
        }
    }

	public static async Task<IFormFile> ConvertImageToPngAsync(IFormFile file)
	{
		if (!IsImageFile(file))
		{
			return file;
		}

		using var inputMemoryStream = new MemoryStream();
		await file.CopyToAsync(inputMemoryStream);
		inputMemoryStream.Position = 0;

		using var image = Image.FromStream(inputMemoryStream);
		var outputMemoryStream = new MemoryStream();
		image.Save(outputMemoryStream, ImageFormat.Png);
		outputMemoryStream.Position = 0;

		var newFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}.png";
		return new FormFile(outputMemoryStream, 0, outputMemoryStream.Length, file.Name, newFileName)
		{
			Headers = file.Headers,
			ContentType = "image/png"
		};
	}

	public static bool IsImageFile(IFormFile file)
	{
		var allowedImageMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/bmp", "image/gif", "image/tiff" };
		return allowedImageMimeTypes.Contains(file.ContentType.ToLowerInvariant());
	}


	public async Task<List<MinIOCloudModel>> UploadFileAsync(List<IFormFile> files, string folderName = "sass", bool? isGroupTenant = false)
	{
		var fileResponse = new List<MinIOCloudModel>();

		try
		{
			if (files.Count <= 0) return fileResponse;

			var isExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(folderName));

			if (!isExists)
			{
				await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(folderName));
			}

			var medias = new List<Media>();
			foreach (var item in files)
			{
				var file = await ConvertImageToPngAsync(item);

				using var stream = new MemoryStream();
				await file.CopyToAsync(stream);
				stream.Position = 0;

				var extension = file.ContentType;
				if (!IsAllowedExtension(extension))
				{
					throw new Exception($"Invalid file type: {extension}");
				}

				var fileName = $"{DateTime.Now.Ticks}_{file.FileName.Replace(" ", "_")}";
				fileName = StringHelper.convertToUnSign(fileName);

				var putObjectArgs = new PutObjectArgs()
					.WithBucket(folderName)
					.WithObject(fileName)
					.WithStreamData(stream)
					.WithObjectSize(stream.Length)
					.WithContentType(file.ContentType);

				await _minioClient.PutObjectAsync(putObjectArgs);

				var statObject = await _minioClient.StatObjectAsync(new StatObjectArgs()
					.WithBucket(folderName)
					.WithObject(fileName));

				var url = GetBaseUrl();
				if (!string.IsNullOrEmpty(statObject.ObjectName))
				{
					var cloudModel = new MinIOCloudModel
					{
						FolderName = folderName,
						OriginalFileName = file.FileName,
						FileName = statObject.ObjectName,
						FileSize = statObject.Size,
						ContentType = statObject.ContentType,
						FilePath = $"http://10.0.0.16:9000/{folderName}/{fileName}",
						ServerPath = $"{url}/api/Storage/{folderName}/{fileName}",
						Extension = extension
					};
					fileResponse.Add(cloudModel);
					if (isGroupTenant == false)
					{
						medias.Add(new Media
						{
							FolderName = folderName,
							OriginalFileName = file.FileName,
							FileName = statObject.ObjectName,
							FileSize = statObject.Size,
							ContentType = statObject.ContentType,
							FilePath = $"http://10.0.0.16:9000/{folderName}/{fileName}",
							ServerPath = $"{url}/api/Storage/{folderName}/{fileName}",
							Extension = extension
						});
					}

				}
			}

			if (medias.Count > 0)
			{
				_context.Medias.AddRange(medias);
				await _context.SaveChangesAsync(_cancellationToken);
			}

			return fileResponse;
		}
		catch (MinioException e)
		{
			throw new ApplicationException("Upload file thất bại");
		}
	}


	public string GetBaseUrl()
	{
        try
        {
			var request = _httpContextAccessor.HttpContext.Request;
			var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
			return baseUrl;
		}
        catch(Exception ex)
        {
            return "https://test01-api.atnavn.com/";

		}
		
	}

    public async Task<MediaResponseDto> DownloadFileAsync(string fileName, string folderName = "sass")
    {
        try
        {
            var memoryStream = new MemoryStream();

            var arg = new StatObjectArgs()
                .WithBucket(folderName)
                .WithObject(fileName);

            var statObject = await _minioClient.StatObjectAsync(arg);
            if (!string.IsNullOrEmpty(statObject.ObjectName))
            {
                var getObjectArgs = new GetObjectArgs()
                    .WithBucket(folderName)
                    .WithObject(fileName)
                    .WithCallbackStream((stream) => { stream.CopyTo(memoryStream); });

                await _minioClient.GetObjectAsync(getObjectArgs);
            }

            memoryStream.Position = 0;

			return new MediaResponseDto
			{
				data = memoryStream.ToArray(),
				Extension = statObject.ContentType
			};
        }
        catch (MinioException e)
        {
            throw new ApplicationException("Tải file thất bại");
        }
    }

    private bool IsAllowedExtension(string extension)
    {
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
            case ".png":
            case ".gif":
            case ".jfif":

            case "image/jpg":
            case "image/jpeg":
            case "image/png":
            case "image/gif":
            case "image/jfif":

			case "application/msword":
            case "application/vnd.ms-excel":
			case "application/vnd.ms-powerpoint":
            case "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet":
            case "application/vnd.openxmlformats-officedocument.wordprocessingml.document":
			case "application/vnd.openxmlformats-officedocument.presentationml.presentation":
            case "application/pdf":

			case "text/csv":
			case "video/x-msvideo":
			case "video/mp4":

            case ".pdf":
            case ".xls":
            case ".xlsx":
            case ".doc":
            case ".docx":
			case ".ppt":
			case ".pptx":
                return true;
            default:
                return false;
        }
    }

    public async Task DeleteMediaAsync(string fileName, string folderName = "sass")
    {
        try
        {
            await _minioClient.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(folderName).WithObject(fileName));
        }
        catch (MinioException e)
        {
            Console.WriteLine($"Error occurred: {e}");
        }
    }
}
