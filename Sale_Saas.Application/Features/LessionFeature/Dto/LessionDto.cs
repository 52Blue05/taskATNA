using Sale_Saas.Application.Features.UnitFeature.Dto;

namespace Sale_Saas.Application.Features.LessionFeature.Dto
{
    public class LessionDto : BaseEntityDto
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
        public bool? IsDone { get; set; }

        public UnitDto? Unit { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Lessions, LessionDto>()
                    .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Units));
            }
        }
    }

    public class LessionDownloadDto
    {
        public byte[] Bytes { get; set; }
        public Lessions Lession { get; set; }
    }
}
