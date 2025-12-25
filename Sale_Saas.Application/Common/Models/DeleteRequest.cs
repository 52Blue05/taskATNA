using Sale_Saas.Domain.Enums;

namespace Sale_Saas.Application.Common.Models;

public class DeleteRequest
{
    public List<string>? Ids { set; get; }
    public Guid ApplicationUserId { get; set; }
    public string? TenantId { get; set; } = "";
    public string? Locale { get; set; } = LocaleEnum.vi_VN.ToString();
}
