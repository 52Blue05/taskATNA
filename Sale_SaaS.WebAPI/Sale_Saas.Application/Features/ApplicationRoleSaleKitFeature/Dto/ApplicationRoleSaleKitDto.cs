namespace Sale_Saas.Application.Features.ApplicationRoleSaleKitFeature.Dto
{
    public class ApplicationRoleSaleKitDto
    {
        public Guid ApplicationRoleId { get; set; }
        public Guid SaleKitId { get; set; }
		public Guid? ParentId { get; set; }
		public string Name { get; set; } = "";
		public string Type { get; set; } = "";
		public string Extension { get; set; } = "";
		public bool Access { get; set; } = false;
		public DateTime CreatedDate { get; set; }
    }
}
