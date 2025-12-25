using Sale_Saas.Application.Models.Identity;

namespace Sale_Saas.Application.Common.Models
{
	public class UpdatePasswordRequest
	{
		public Guid id { get; set; }
        //public string? CurrentPassword { get; set; } = string.Empty;
        //public string NewPassword { get; set; } = string.Empty;
        //public string? ConfirmPassword { get; set; } = string.Empty;
        public Dictionary<string, string>? Data { set; get; }
    }

    public class UpdatePasswordMobileRequest
    {
        public string? CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string? ConfirmPassword { get; set; } = string.Empty;
    }

    public class UpdatePasswordByOtpMobileRequest : SendOtpRequest
    {
        public string? CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string? ConfirmPassword { get; set; } = string.Empty;
    }

    public class UpdatePasswordDto
    {
        public Guid Id { get; set; }
        public string? NewPassword { get; set; } = string.Empty;
        public string? ConfirmPassword { get; set; } = string.Empty;
    }
}
