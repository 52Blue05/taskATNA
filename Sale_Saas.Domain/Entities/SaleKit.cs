namespace Sale_Saas.Domain.Entities
{
    public class SaleKit : BaseAuditableEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? FolderName { get; set; }
        public string? OriginalFileName { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }
        public string? FilePath { get; set; }
		public string? ServerPath { get; set; }
        public string? Extension { get; set; }
		public string? Type { get; set; }
        public Guid? ParentId { get; set; }
        public ICollection<ApplicationRoleSaleKit>? RoleSaleKits { get; set; }
    }
}
