
namespace Sale_Saas.Application.Features.UnitFeature.Dto
{
    public class UnitDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? ThumbnailPath { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Description { get; set; }
        public int? SortOrder { get; set; }
    }

    public class UnitDetailDto : UnitDto
    {
        public bool? IsLove { get; set; }
    }

    public class ListUnitToSyllabusDto
    {
        public Guid? UnitId { get; set; }
        public string? Name { get; set; }
    }

    public class AddUnit
    {
        //public Guid? SyllabusId { get; set; }
        public string? Name { get; set; }
        public IFormFile? Thumbnail { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateUnit : AddUnit
    {
        public Guid? Id { get; set; }
    }

    public class UnitResultDto : BaseEntityDto
    {
        public string Name { get; set; }
        public int? CorrectQuestion { get; set; }
        public int? TotalQuestion { get; set; }
        public bool? isQualified { get; set; }
    }

    public class UnitOfMemberDto : UnitDto
    {
        public bool? isQualified { get; set; }
        public int? SortOrder { get; set; }
    }
}
