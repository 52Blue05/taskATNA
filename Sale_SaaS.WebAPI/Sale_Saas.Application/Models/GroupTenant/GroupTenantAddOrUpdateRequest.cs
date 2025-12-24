namespace Sale_Saas.Application.Models.GroupTenant
{
	public class GroupTenantAddOrUpdateBaseRequest
	{
        public Guid? Id { get; set; }
        public string? Code { get; set; }
        public string? CompanyName { get; set; }
        public string? FullName { get; set; }
        public IFormFile? Logo { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Domain { get; set; }
        public string? Color1 { get; set; }
        public string? Color2 { get; set; }
    }
	public class GroupTenantAddOrUpdateRequest : GroupTenantAddOrUpdateBaseRequest
	{
        public string? Password { get; set; }
        public Guid? PlanServiceId { get; set; }
    }

    public class GroupTenantUpdatePlanServiceRequest
    {
        public Guid GroupTenantId { get; set; }
        public Guid PlanServiceId { get; set; }
    }
}
