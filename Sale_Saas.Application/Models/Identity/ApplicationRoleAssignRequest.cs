namespace Sale_Saas.Application.Models.Identity;
public class ApplicationRoleAssignRequest
{
    public Guid? Id { get; set; }
    public List<Guid>? ApplicationRoleIds { get; set; } = new List<Guid>();
    public bool? IsAdmin { get; set; }
}