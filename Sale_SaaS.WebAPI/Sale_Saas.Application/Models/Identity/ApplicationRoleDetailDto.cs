namespace Sale_Saas.Application.Models.Identity;

public class ApplicationRoleDetailDto
{
    public Guid? MenuId { set; get; }
    public int? Permision { set; get; }
    public string? NameController { set; get; }
    public string? NameAction { set; get; }
}
