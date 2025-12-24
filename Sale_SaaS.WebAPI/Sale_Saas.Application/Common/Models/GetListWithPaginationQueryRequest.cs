
using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Common.Models
{
    public class GetListWithPaginationQueryRequest
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string? TextSearch { get; set; }
        public string? OrderCol { get; set; }
        public string? OrderDir { get; set; }
        public string? Code { set; get; }
        public Guid? UserId { get; set; }
        public Guid? StatusId { get; set; }
        public Guid? RoleId { get; set; }
        public DateTime? Time { get; set; }
        public string? RoleType { get; set; }
        public string? ConnectString { get; set; }
        public string? TenantId { get; set; }
        public Guid? GroupTenantId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? BenefitId { get; set; }
        public string? Roles { get; set; }
    }
}
