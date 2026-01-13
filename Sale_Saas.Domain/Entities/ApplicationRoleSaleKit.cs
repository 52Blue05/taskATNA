namespace Sale_Saas.Domain.Entities
{
    public class ApplicationRoleSaleKit : BaseAuditableEntity
    {
        public Guid? ApplicationRoleId { get; set; }
        public ApplicationRole? ApplicationRole { get; set; }
        public Guid? SaleKitId { get; set; }
        public SaleKit? SaleKit { get; set; }
        public bool? Access { get; set; }
    }
}
