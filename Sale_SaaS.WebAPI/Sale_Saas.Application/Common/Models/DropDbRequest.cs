

namespace Sale_Saas.Application.Common.Models
{
    public class DropDbRequest
    {
        public string? TenantId { get; set; }
        public string? Password { get; set; }
        public List<string>? ListTenantId { get; set; }
    }
}
