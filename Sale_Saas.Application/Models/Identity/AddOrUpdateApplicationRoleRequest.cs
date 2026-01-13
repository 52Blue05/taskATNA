namespace Sale_Saas.Application.Models.Identity;

public class AddOrUpdateApplicationRoleRequest: AddOrUpdateRequest
{
    public List<ApplicationRoleDetailRequest>? ApplicationRoleDetails{ set; get; }
}
