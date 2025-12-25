
namespace Sale_Saas.Domain.Entities
{
    public class Units : BaseAuditableEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ThumbnailPath { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public ICollection<SyllabusUnits>? SyllabusUnits { get; set; }
        public ICollection<LoveUnits>? LoveUnits { get; set; }
        public ICollection<Lessions>? Lessions { get; set; }
        public ICollection<UnitQuestions>? UnitQuestions { get; set; }
        public ICollection<Result>? Results { get; set; }
    }
}
