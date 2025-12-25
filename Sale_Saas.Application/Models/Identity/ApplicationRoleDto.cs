namespace Sale_Saas.Application.Models.Identity;

public class ApplicationRoleDto
{
    public Guid Id { set; get; }
    public string Name { set; get; }
    public string DisplayName { set; get; }
    public string Description { set; get; }
    public Guid ApplicationUserId { set; get; }
    public string RolePositionId { get; set; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<ApplicationRole, ApplicationRoleDto>();
        }
    }
}

public class IncomeRoleDto
{
    public Guid Id { set; get; }
    public string DisplayName { set; get; }
    public decimal? Income { set; get; }

}

public class RoleDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; }
    public string RolePositionId { get; set; }
    public string Name { get; set; }
}
public class UserRoleDto
{
    public Guid RoleId { set; get; }
    public Guid UserId { set; get; }
}