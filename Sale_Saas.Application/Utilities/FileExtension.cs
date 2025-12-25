using Microsoft.AspNetCore.Http.Internal;
using System.Drawing;
using System.Drawing.Imaging;

namespace Sale_Saas.Application.Utilities
{
	public static class FileExtension
	{
		public static async Task<IFormFile> ConvertImageToPngAsync(IFormFile file)
		{
			if (!IsImageFile(file))
			{
				return file;
			}

			using var memoryStream = new MemoryStream();
			await file.CopyToAsync(memoryStream);
			memoryStream.Position = 0;

			using var image = Image.FromStream(memoryStream);
			using var pngStream = new MemoryStream();
			image.Save(pngStream, ImageFormat.Png);
			pngStream.Position = 0;

			var newFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}.png";
			var convertedImage = new FormFile(pngStream, 0, pngStream.Length, file.Name, newFileName)
			{
				Headers = file.Headers,
				ContentType = "image/png"
			};

			return convertedImage;
		}

		public static bool IsImageFile(IFormFile file)
		{
			var allowedImageMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/bmp", "image/gif", "image/tiff" };
			return allowedImageMimeTypes.Contains(file.ContentType.ToLowerInvariant());
		}
	}
}
