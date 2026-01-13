namespace Sale_Saas.Application.Common.Models;

public class TenantWithAllUserDto
{
    public string UserName { get; set; }
    public string TenantId { get; set; }
    public string TenantName { get; set; }
    public string? ConnectString { get; set; }
    public Guid? ApplicationUserId { get; set; }
}