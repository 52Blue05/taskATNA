using AutoMapper;

namespace Sale_Saas.Domain.Entities
{
    public class ProjectStatus : BaseAuditableEntity
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public ICollection<Project>? Projects { set; get; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<ProjectStatus, ProjectStatus>()
                    .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code ?? ""))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name ?? ""));
            }
        }
    }
}
