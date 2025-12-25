using Sale_Saas.Application.Features.UnitFeature.Dto;

namespace Sale_Saas.Application.Features.SyllabusFeature.Dto
{
    public class SyllabusAdminDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public int? ToTalUnit { get; set; }
        public int? ToTalUser { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class SyllabusAdminResult
    {
        public string? Status { get; set; }
        public int? MaxQuestion { get; set; }
        public int? CorrectQuestion { get; set; }
    }

    public class SyllabusAdminUserDto
    {
        public Guid? Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public List<SyllabusAdminResult> listSyllabusResults { get; set; } = new List<SyllabusAdminResult>();
    }

    public class AddSyllabus
    {
        public string? Name { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class UpdateSyllabus : AddSyllabus
    {
        public Guid? Id { get; set; }
    }

    public class AddUserToSyllabus
    {
        public Guid? syllabusId { get; set; }
        public List<string>? listEmailUser { get; set; }
    }

    public class AddUnitToSyllabus
    {
        public Guid? SyllabusId { get; set; }
        public List<Guid>? ListUnitId { get; set; }
    }

    public class DeleteUnitFromSyllabus
    {
        public Guid? SyllabusId { get; set; }
        public Guid? UnitId { get; set; }
    }

    public class SyllabusUserDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public int? ToTalUnit { get; set; }
        public int? ToTalLesson { get; set; }
        public DateTime? EndTime { get; set; }
        public bool? IsQualified { get; set; }
    }

    public class SyllabusMemberDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }

        public List<UnitResultDto> UnitResults { get; set; }
    }

    public class SyllabusUnitDto
    {
        public Guid Id { get; set; }
        public Guid UnitId { get; set; }
        public string UnitName { get; set; }
        public int SortOrder { get; set; }
    }

    public class SyllabusUnitOfUserDto
    {
        public Guid Id { get; set; }
        public string SyllabusName { get; set; } = "";
        public string FullName { get; set; } = "";
        public List<UnitOfUserDto> Units { get; set; } = new List<UnitOfUserDto>();

    }

    public class UnitOfUserDto
    {
        public Guid Id { get; set; }
        public string UnitName { get; set; } = "";
        public bool? isQualified { get; set; }
        public string ThumbnailPath { get; set; } = "";
        public DateTime? EndTime { get; set; }
        public string Description { get; set; } = "";
        public DateTime? CompletionDate { get; set; }
        public int? CorrectQuestion { get; set; }
        public int? TotalQuestion { get; set; }
    }
}
