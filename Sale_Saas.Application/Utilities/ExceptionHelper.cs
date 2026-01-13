using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Utilities
{
	public static class ExceptionHelper
	{
		private static string vi_VN { get; } = LocaleEnum.vi_VN.ToString();
		private static string en_US { get; } = LocaleEnum.en_US.ToString();
		public static ApplicationException NotFound(object obj, string? locale = null)
		{
			locale ??= vi_VN;
			string msg = $"Không tìm thấy dữ liệu với giá trị: {obj}.";
			if (locale == en_US)
				msg = $"Data not found with value: {obj}.";
			throw new ApplicationException(msg);
		}

		public static ApplicationException RequestEmpty(string? locale = null)
		{
			locale ??= vi_VN;
			string msg = $"Không có dữ liệu gửi đến máy chủ.";
			if (locale == en_US)
				msg = $"No data sent to the server.";
			throw new ApplicationException(msg);
		}

		public static ApplicationException AlreadyUsed(object obj, string value, string? locale = null)
		{
			locale ??= vi_VN;
			string msg = $"{value}: {obj} đã được sử dụng, vui lòng chọn {value} khác.";
			if (locale == en_US)
				msg = "${value}: ${obj} has been used, please choose another ${value}.";
			throw new ApplicationException(msg);
		}

		public static ApplicationException DataAlreadyUsed(object obj, string value = "", string? locale = null)
		{
			locale ??= vi_VN;
			string msg = $"{value}: {obj} đã được sử dụng, vui lòng chọn {value} khác.";
			if (locale == en_US)
				msg = "${value}: ${obj} has been used, please choose another ${value}.";
			throw new ApplicationException(msg);
		}

		public static ApplicationException NotAccess(string? locale = null)
		{
			locale ??= vi_VN;
			string msg = $"Tài khoản của bạn không được quyền thao tác.";
			if (locale == en_US)
				msg = "Your account is not authorized to perform this action.";
			throw new ApplicationException(msg);
		}

		public static ApplicationException StatusInvalid(object obj, string? locale = null)
		{
			locale ??= vi_VN;
			string msg = $"Trạng thái không hợp lệ: {obj}";
			if (locale == en_US)
				msg = $"Invalid data status: {obj}";
			throw new ApplicationException(msg);
		}
	}
}
