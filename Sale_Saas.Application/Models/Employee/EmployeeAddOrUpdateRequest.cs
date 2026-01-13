using Sale_Saas.Application.Models.Identity;
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Models.Employee
{
	public class EmployeeAddOrUpdateRequest
	{
		public Guid? Id { set; get; }
		public EmployeeUpdateDto? Data { set; get; }
		public Guid? CreatedApplicationUserId { set; get; }
		public Guid? LastModifiedApplicationUserId { set; get; }
		public string? Locale { get; set; } = LocaleEnum.vi_VN.ToString();
	}
}
