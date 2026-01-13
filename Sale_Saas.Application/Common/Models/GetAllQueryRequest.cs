using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Common.Models
{
    public class GetAllQueryRequest
    {
        public string? TextSearch { get; set; }
        public string? OrderCol { get; set; }
        public string? OrderDir { get; set; }
        public Guid? UserId { get; set; }
        public Guid? RoleId { get; set; }
        public string? RoleType { get; set;}
		public string? RolePositionId { get; set; }
		public string? TenantId { get; set; }
    }
}
