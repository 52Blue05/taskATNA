
using System.ComponentModel.DataAnnotations.Schema;

namespace Sale_Saas.Domain.Entities
{
    public class Lessions : BaseAuditableEntity
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
        public bool? IsFile { get; set; }
        public string? Link { get; set; }
        public int? SortOrder { get; set; }

        public Guid UnitId { get; set; }
        [ForeignKey("UnitId")]
        public Units? Units { get; set; }
    }
}
