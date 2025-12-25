namespace Sale_Saas.Application.Features.ProjectStatusFeature.Dto
{
    public class ProjectStatusDto : BaseEntityDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ProjectStatus, ProjectStatusDto>()
                    .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code ?? ""))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name ?? ""));
            }
        }
    }
}
