using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Common.Models
{
	public class FilterQueryRequest
	{
		public string? Code { set; get; }
		public string? Name { set; get; }
		public int? Skip { get; set; }
		public int? TotalRecord { get; set; }
		public string? TextSearch { get; set; }
		public string? OrderCol { get; set; }
		public string? OrderDir { get; set; }
		public bool? StartWith { get; set; }
		public bool? NoGetDataWhenTextSearchIsEmpty { get; set; }
		public DateTime? Time { get; set; }
        public Guid? UserId { get; set; }
        public Guid? RoleId { get; set; }
		public string? RoleType { get; set; }
		public string? TenantId { get; set; }
		public Guid? GroupTenantId { get; set; }
	}
}
